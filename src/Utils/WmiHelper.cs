using System;
using System.Collections.Generic;
using System.Management;
using PowerPlanTools.Models;

namespace PowerPlanTools.Utils
{
    /// <summary>
    /// Helper class for WMI operations related to power plans
    /// </summary>
    internal static class WmiHelper
    {
        /// <summary>
        /// WMI namespace for power management
        /// </summary>
        private const string PowerManagementNamespace = @"root\cimv2\power";

        /// <summary>
        /// Gets all power plans from WMI
        /// </summary>
        /// <returns>A list of PowerPlan objects</returns>
        public static List<PowerPlan> GetPowerPlans()
        {
            List<PowerPlan> powerPlans = new List<PowerPlan>();
            Guid activePlanGuid = GetActivePowerPlanGuid();

            using (ManagementObjectSearcher searcher = new ManagementObjectSearcher(PowerManagementNamespace, "SELECT * FROM Win32_PowerPlan"))
            {
                foreach (ManagementObject plan in searcher.Get())
                {
                    string elementName = plan["ElementName"] as string;
                    string instanceId = plan["InstanceID"] as string;
                    string description = plan["Description"] as string;

                    // Extract GUID from InstanceID (format: "Microsoft:PowerPlan\\{GUID}")
                    Guid planGuid = ExtractGuidFromInstanceId(instanceId);
                    bool isActive = planGuid == activePlanGuid;

                    PowerPlan powerPlan = new PowerPlan(elementName, planGuid, isActive, description);
                    powerPlans.Add(powerPlan);
                }
            }

            return powerPlans;
        }

        /// <summary>
        /// Gets the active power plan GUID
        /// </summary>
        /// <returns>The GUID of the active power plan</returns>
        public static Guid GetActivePowerPlanGuid()
        {
            using (ManagementObjectSearcher searcher = new ManagementObjectSearcher(PowerManagementNamespace, "SELECT * FROM Win32_PowerPlan WHERE IsActive = True"))
            {
                foreach (ManagementObject plan in searcher.Get())
                {
                    string instanceId = plan["InstanceID"] as string;
                    return ExtractGuidFromInstanceId(instanceId);
                }
            }

            return Guid.Empty;
        }

        /// <summary>
        /// Sets the active power plan
        /// </summary>
        /// <param name="planGuid">The GUID of the power plan to activate</param>
        /// <returns>True if successful, false otherwise</returns>
        public static bool SetActivePowerPlan(Guid planGuid)
        {
            string query = $"SELECT * FROM Win32_PowerPlan WHERE InstanceID = 'Microsoft:PowerPlan\\\\{{{planGuid.ToString().ToUpper()}}}'";

            using (ManagementObjectSearcher searcher = new ManagementObjectSearcher(PowerManagementNamespace, query))
            {
                foreach (ManagementObject plan in searcher.Get())
                {
                    ManagementBaseObject outParams = plan.InvokeMethod("Activate", null, null);
                    return outParams == null || (uint)outParams["ReturnValue"] == 0;
                }
            }

            return false;
        }

        /// <summary>
        /// Gets power settings for a specific power plan
        /// </summary>
        /// <param name="planGuid">The GUID of the power plan</param>
        /// <param name="includeHidden">Whether to include hidden settings</param>
        /// <returns>A list of PowerSetting objects</returns>
        public static List<PowerSetting> GetPowerSettings(Guid planGuid, bool includeHidden = false)
        {
            List<PowerSetting> settings = new List<PowerSetting>();

            // Get all power settings for the specified plan
            string query = $"SELECT * FROM Win32_PowerSettingDataIndex WHERE InstanceID LIKE 'Microsoft:PowerSettingDataIndex\\\\{{{planGuid.ToString().ToUpper()}}}%'";

            using (ManagementObjectSearcher searcher = new ManagementObjectSearcher(PowerManagementNamespace, query))
            {
                foreach (ManagementObject settingData in searcher.Get())
                {
                    string instanceId = settingData["InstanceID"] as string;

                    // Extract setting GUID and subgroup GUID from InstanceID
                    // Format: "Microsoft:PowerSettingDataIndex\\{PlanGuid}\\{SubGroupGuid}\\{SettingGuid}"
                    string[] parts = instanceId.Split('\\');
                    if (parts.Length < 5)
                        continue;

                    Guid subGroupGuid = ExtractGuidFromPart(parts[3]);
                    Guid settingGuid = ExtractGuidFromPart(parts[4]);

                    // Get setting metadata
                    PowerSetting setting = GetPowerSettingMetadata(settingGuid, subGroupGuid);
                    if (setting == null)
                        continue;

                    // Skip hidden settings if not requested
                    if (setting.IsHidden && !includeHidden)
                        continue;

                    // Get AC and DC values
                    setting.PluggedIn = settingData["ACSettingIndex"];
                    setting.OnBattery = settingData["DCSettingIndex"];

                    settings.Add(setting);
                }
            }

            return settings;
        }

        /// <summary>
        /// Gets metadata for a power setting
        /// </summary>
        /// <param name="settingGuid">The GUID of the setting</param>
        /// <param name="subGroupGuid">The GUID of the subgroup</param>
        /// <returns>A PowerSetting object with metadata</returns>
        private static PowerSetting GetPowerSettingMetadata(Guid settingGuid, Guid subGroupGuid)
        {
            // First check if the setting is in our known settings dictionary
            string alias = "Unknown Setting";
            string description = string.Empty;

            string knownName = ArgumentCompleters.GetPowerSettingAlias(settingGuid);
            if (knownName != settingGuid.ToString())
            {
                alias = knownName;
            }

            // Try to get additional metadata from WMI
            string query = $"SELECT * FROM Win32_PowerSetting WHERE InstanceID = 'Microsoft:PowerSetting\\\\{{{settingGuid.ToString().ToUpper()}}}'";

            using (ManagementObjectSearcher searcher = new ManagementObjectSearcher(PowerManagementNamespace, query))
            {
                foreach (ManagementObject settingObj in searcher.Get())
                {
                    // Only use WMI name if we don't have a known name
                    if (ArgumentCompleters.GetPowerSettingAlias(settingGuid) == settingGuid.ToString())
                    {
                        string elementName = settingObj["ElementName"] as string;
                        if (!string.IsNullOrEmpty(elementName))
                        {
                            alias = elementName;
                        }
                    }

                    description = settingObj["Description"] as string;
                    break;
                }
            }

            PowerSetting setting = new PowerSetting
            {
                Alias = alias,
                SettingGuid = settingGuid,
                SubGroupGuid = subGroupGuid,
                SubGroupAlias = ArgumentCompleters.GetSubgroupAlias(subGroupGuid),
                Description = description,
                Units = GetPowerSettingUnits(settingGuid)
            };

            // Check if setting is hidden
            setting.IsHidden = IsHiddenSetting(settingGuid);

            return setting;
        }

        /// <summary>
        /// Gets the units for a power setting
        /// </summary>
        /// <param name="settingGuid">The GUID of the setting</param>
        /// <returns>The units string</returns>
        private static string GetPowerSettingUnits(Guid settingGuid)
        {
            // First try to get units from WMI
            string units = string.Empty;
            string query = $"SELECT * FROM Win32_PowerSettingDefinitionData WHERE SettingGuid = '{{{settingGuid.ToString().ToUpper()}}}'";

            using (ManagementObjectSearcher searcher = new ManagementObjectSearcher(PowerManagementNamespace, query))
            {
                foreach (ManagementObject unitObj in searcher.Get())
                {
                    units = unitObj["UnitSpecifier"] as string;
                    if (!string.IsNullOrEmpty(units))
                    {
                        return units;
                    }
                }
            }

            // If WMI doesn't have units, use our hardcoded mapping
            switch (settingGuid.ToString().ToLower())
            {
                // Display brightness settings
                case "aded5e82-b909-4619-9949-f5d71dac0bcb": // Display brightness
                case "f1fbfde2-a960-4165-9f88-50667911ce96": // Dimmed display brightness
                    return "%";

                // Sleep timeout settings
                case "29f6c1db-86da-48c5-9fdb-f2b67b1f44da": // Sleep after (AC)
                case "9d7815a6-7ee4-497e-8888-515a05f02364": // Sleep after (DC)
                case "bd3b718a-0680-4d9d-8ab2-e1d2b4ac806d": // Hybrid sleep
                case "d4e98f31-5ffe-4ce1-be31-1b38b384c009": // Hibernate after
                    return "seconds";

                // Processor power settings
                case "bc5038f7-23e0-4960-96da-33abaf5935ec": // Maximum processor state
                case "893dee8e-2bef-41e0-89c6-b55d0929964c": // Minimum processor state
                    return "%";

                // Battery settings
                case "e69653ca-cf7f-4f05-aa73-cb833fa90ad4": // Critical battery level
                case "9a66d8d7-4ff7-4ef9-b5a2-5a326ca2a469": // Low battery level
                    return "%";

                // Default to empty string for unknown units
                default:
                    return string.Empty;
            }
        }

        /// <summary>
        /// Determines if a setting is hidden
        /// </summary>
        /// <param name="settingGuid">The GUID of the setting</param>
        /// <returns>True if the setting is hidden, false otherwise</returns>
        private static bool IsHiddenSetting(Guid settingGuid)
        {
            // This is a simplified implementation
            // In a real implementation, you would check registry or other sources
            // to determine if a setting is hidden
            return false;
        }

        /// <summary>
        /// Updates a power setting value
        /// </summary>
        /// <param name="planGuid">The GUID of the power plan</param>
        /// <param name="settingGuid">The GUID of the setting</param>
        /// <param name="subGroupGuid">The GUID of the subgroup</param>
        /// <param name="acValue">The value when plugged in</param>
        /// <param name="dcValue">The value when on battery</param>
        /// <returns>True if successful, false otherwise</returns>
        public static bool UpdatePowerSetting(Guid planGuid, Guid settingGuid, Guid subGroupGuid, object acValue, object dcValue)
        {
            string query = $"SELECT * FROM Win32_PowerSettingDataIndex WHERE InstanceID = 'Microsoft:PowerSettingDataIndex\\\\{{{planGuid.ToString().ToUpper()}}}\\\\{{{subGroupGuid.ToString().ToUpper()}}}\\\\{{{settingGuid.ToString().ToUpper()}}}'";

            using (ManagementObjectSearcher searcher = new ManagementObjectSearcher(PowerManagementNamespace, query))
            {
                foreach (ManagementObject settingData in searcher.Get())
                {
                    if (acValue != null)
                        settingData["ACSettingIndex"] = acValue;

                    if (dcValue != null)
                        settingData["DCSettingIndex"] = dcValue;

                    settingData.Put();
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Creates a new power plan by cloning an existing one
        /// </summary>
        /// <param name="sourcePlanGuid">The GUID of the source power plan</param>
        /// <param name="newPlanName">The name for the new power plan</param>
        /// <returns>The GUID of the new power plan, or Guid.Empty if failed</returns>
        public static Guid CreatePowerPlan(Guid sourcePlanGuid, string newPlanName)
        {
            // This is a simplified implementation
            // In a real implementation, you would use the Win32_PowerPlan.CreatePowerPlan method
            // or PowrProf.dll interop to create a new power plan
            return Guid.Empty;
        }

        /// <summary>
        /// Deletes a power plan
        /// </summary>
        /// <param name="planGuid">The GUID of the power plan to delete</param>
        /// <returns>True if successful, false otherwise</returns>
        public static bool DeletePowerPlan(Guid planGuid)
        {
            string query = $"SELECT * FROM Win32_PowerPlan WHERE InstanceID = 'Microsoft:PowerPlan\\\\{{{planGuid.ToString().ToUpper()}}}'";

            using (ManagementObjectSearcher searcher = new ManagementObjectSearcher(PowerManagementNamespace, query))
            {
                foreach (ManagementObject plan in searcher.Get())
                {
                    plan.Delete();
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Extracts a GUID from an InstanceID string
        /// </summary>
        /// <param name="instanceId">The InstanceID string</param>
        /// <returns>The extracted GUID</returns>
        private static Guid ExtractGuidFromInstanceId(string instanceId)
        {
            if (string.IsNullOrEmpty(instanceId))
                return Guid.Empty;

            int startIndex = instanceId.IndexOf('{');
            int endIndex = instanceId.IndexOf('}');

            if (startIndex >= 0 && endIndex > startIndex)
            {
                string guidString = instanceId.Substring(startIndex, endIndex - startIndex + 1);
                if (Guid.TryParse(guidString, out Guid result))
                    return result;
            }

            return Guid.Empty;
        }

        /// <summary>
        /// Extracts a GUID from a part of an InstanceID string
        /// </summary>
        /// <param name="part">The part of the InstanceID string</param>
        /// <returns>The extracted GUID</returns>
        private static Guid ExtractGuidFromPart(string part)
        {
            if (string.IsNullOrEmpty(part))
                return Guid.Empty;

            if (Guid.TryParse(part, out Guid result))
                return result;

            if (part.StartsWith("{") && part.EndsWith("}"))
            {
                if (Guid.TryParse(part, out result))
                    return result;
            }

            return Guid.Empty;
        }
    }
}
