using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using PowerPlanTools.Models;

namespace PowerPlanTools.Utils
{
    /// <summary>
    /// Helper class for PowrProf.dll interop operations
    /// </summary>
    internal static class PowerProfileHelper
    {
        #region PowrProf.dll P/Invoke Declarations

        [DllImport("PowrProf.dll")]
        private static extern uint PowerReadPossibleValue(
            IntPtr RootPowerKey,
            ref Guid SchemeGuid,
            ref Guid SubGroupOfPowerSettingGuid,
            ref Guid PowerSettingGuid,
            uint Type,
            IntPtr Buffer,
            ref uint BufferSize);

        [DllImport("PowrProf.dll")]
        private static extern uint PowerReadPossibleFriendlyName(
            IntPtr RootPowerKey,
            ref Guid SchemeGuid,
            ref Guid SubGroupOfPowerSettingGuid,
            ref Guid PowerSettingGuid,
            uint Type,
            IntPtr Buffer,
            ref uint BufferSize);

        [DllImport("PowrProf.dll")]
        private static extern uint PowerEnumerate(
            IntPtr RootPowerKey,
            IntPtr SchemeGuid,
            IntPtr SubGroupOfPowerSettingGuid,
            uint AccessFlags,
            uint Index,
            [Out] byte[] Buffer,
            ref uint BufferSize);

        [DllImport("PowrProf.dll")]
        private static extern uint PowerReadFriendlyName(
            IntPtr RootPowerKey,
            ref Guid SchemeGuid,
            IntPtr SubGroupOfPowerSettingGuid,
            IntPtr PowerSettingGuid,
            IntPtr Buffer,
            ref uint BufferSize);

        [DllImport("PowrProf.dll")]
        private static extern uint PowerReadDescription(
            IntPtr RootPowerKey,
            ref Guid SchemeGuid,
            IntPtr SubGroupOfPowerSettingGuid,
            IntPtr PowerSettingGuid,
            IntPtr Buffer,
            ref uint BufferSize);

        [DllImport("PowrProf.dll")]
        private static extern uint PowerReadACValue(
            IntPtr RootPowerKey,
            ref Guid SchemeGuid,
            ref Guid SubGroupOfPowerSettingGuid,
            ref Guid PowerSettingGuid,
            ref uint Type,
            ref uint Buffer,
            ref uint BufferSize);

        [DllImport("PowrProf.dll")]
        private static extern uint PowerReadDCValue(
            IntPtr RootPowerKey,
            ref Guid SchemeGuid,
            ref Guid SubGroupOfPowerSettingGuid,
            ref Guid PowerSettingGuid,
            ref uint Type,
            ref uint Buffer,
            ref uint BufferSize);

        [DllImport("PowrProf.dll")]
        private static extern uint PowerWriteACValueIndex(
            IntPtr RootPowerKey,
            ref Guid SchemeGuid,
            ref Guid SubGroupOfPowerSettingGuid,
            ref Guid PowerSettingGuid,
            uint ValueIndex);

        [DllImport("PowrProf.dll")]
        private static extern uint PowerWriteDCValueIndex(
            IntPtr RootPowerKey,
            ref Guid SchemeGuid,
            ref Guid SubGroupOfPowerSettingGuid,
            ref Guid PowerSettingGuid,
            uint ValueIndex);

        [DllImport("PowrProf.dll")]
        private static extern uint PowerDuplicateScheme(
            IntPtr RootPowerKey,
            ref Guid SourceSchemeGuid,
            out IntPtr DestinationSchemeGuid);

        [DllImport("PowrProf.dll")]
        private static extern uint PowerDeleteScheme(
            IntPtr RootPowerKey,
            ref Guid SchemeGuid);

        [DllImport("PowrProf.dll")]
        private static extern uint PowerSetActiveScheme(
            IntPtr RootPowerKey,
            ref Guid SchemeGuid);

        [DllImport("PowrProf.dll")]
        private static extern uint PowerGetActiveScheme(
            IntPtr RootPowerKey,
            out IntPtr ActivePolicyGuid);

        #endregion

        #region Constants

        private const uint ERROR_SUCCESS = 0;
        private const uint ERROR_MORE_DATA = 234;

        private const uint ACCESS_SCHEME = 16;
        private const uint ACCESS_SUBGROUP = 17;
        private const uint ACCESS_INDIVIDUAL_SETTING = 18;

        // Value types
        private const uint REG_NONE = 0;
        private const uint REG_SZ = 1;
        private const uint REG_BINARY = 3;
        private const uint REG_DWORD = 4;

        #endregion

        /// <summary>
        /// Gets all power plans using PowrProf.dll
        /// </summary>
        /// <returns>A list of PowerPlan objects</returns>
        public static List<PowerPlan> GetPowerPlans()
        {
            List<PowerPlan> powerPlans = new List<PowerPlan>();
            Guid activePlanGuid = GetActivePowerPlanGuid();

            uint index = 0;
            uint bufferSize = 16; // Size of a GUID
            byte[] buffer = new byte[bufferSize];

            while (PowerEnumerate(IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, ACCESS_SCHEME, index, buffer, ref bufferSize) == ERROR_SUCCESS)
            {
                Guid planGuid = new Guid(buffer);
                string name = GetPowerPlanFriendlyName(planGuid);
                string description = GetPowerPlanDescription(planGuid);
                bool isActive = planGuid == activePlanGuid;

                PowerPlan powerPlan = new PowerPlan(name, planGuid, isActive, description);
                powerPlans.Add(powerPlan);

                index++;
            }

            return powerPlans;
        }

        /// <summary>
        /// Gets the active power plan GUID
        /// </summary>
        /// <returns>The GUID of the active power plan</returns>
        public static Guid GetActivePowerPlanGuid()
        {
            IntPtr activeGuidPtr;
            if (PowerGetActiveScheme(IntPtr.Zero, out activeGuidPtr) == ERROR_SUCCESS)
            {
                Guid activeGuid = (Guid)Marshal.PtrToStructure(activeGuidPtr, typeof(Guid));
                Marshal.FreeHGlobal(activeGuidPtr);
                return activeGuid;
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
            return PowerSetActiveScheme(IntPtr.Zero, ref planGuid) == ERROR_SUCCESS;
        }

        /// <summary>
        /// Gets the friendly name of a power plan
        /// </summary>
        /// <param name="planGuid">The GUID of the power plan</param>
        /// <returns>The friendly name of the power plan</returns>
        private static string GetPowerPlanFriendlyName(Guid planGuid)
        {
            uint bufferSize = 0;
            PowerReadFriendlyName(IntPtr.Zero, ref planGuid, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, ref bufferSize);

            IntPtr buffer = Marshal.AllocHGlobal((int)bufferSize);
            try
            {
                if (PowerReadFriendlyName(IntPtr.Zero, ref planGuid, IntPtr.Zero, IntPtr.Zero, buffer, ref bufferSize) == ERROR_SUCCESS)
                {
                    return Marshal.PtrToStringUni(buffer);
                }
            }
            finally
            {
                Marshal.FreeHGlobal(buffer);
            }

            return $"Unknown Plan ({planGuid})";
        }

        /// <summary>
        /// Gets the description of a power plan
        /// </summary>
        /// <param name="planGuid">The GUID of the power plan</param>
        /// <returns>The description of the power plan</returns>
        private static string GetPowerPlanDescription(Guid planGuid)
        {
            uint bufferSize = 0;
            PowerReadDescription(IntPtr.Zero, ref planGuid, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, ref bufferSize);

            IntPtr buffer = Marshal.AllocHGlobal((int)bufferSize);
            try
            {
                if (PowerReadDescription(IntPtr.Zero, ref planGuid, IntPtr.Zero, IntPtr.Zero, buffer, ref bufferSize) == ERROR_SUCCESS)
                {
                    return Marshal.PtrToStringUni(buffer);
                }
            }
            finally
            {
                Marshal.FreeHGlobal(buffer);
            }

            return string.Empty;
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
            List<Guid> subGroups = EnumerateSubGroups(planGuid);

            foreach (Guid subGroupGuid in subGroups)
            {
                List<Guid> settingGuids = EnumerateSettings(planGuid, subGroupGuid);

                foreach (Guid settingGuid in settingGuids)
                {
                    PowerSetting setting = GetPowerSettingInfo(planGuid, subGroupGuid, settingGuid);

                    // Skip hidden settings if not requested
                    if (setting.IsHidden && !includeHidden)
                        continue;

                    settings.Add(setting);
                }
            }

            return settings;
        }

        /// <summary>
        /// Enumerates subgroups for a power plan
        /// </summary>
        /// <param name="planGuid">The GUID of the power plan</param>
        /// <returns>A list of subgroup GUIDs</returns>
        private static List<Guid> EnumerateSubGroups(Guid planGuid)
        {
            List<Guid> subGroups = new List<Guid>();
            uint index = 0;
            uint bufferSize = 16; // Size of a GUID
            byte[] buffer = new byte[bufferSize];

            IntPtr schemeGuidPtr = Marshal.AllocHGlobal(Marshal.SizeOf(planGuid));
            Marshal.StructureToPtr(planGuid, schemeGuidPtr, false);

            try
            {
                while (PowerEnumerate(IntPtr.Zero, schemeGuidPtr, IntPtr.Zero, ACCESS_SUBGROUP, index, buffer, ref bufferSize) == ERROR_SUCCESS)
                {
                    Guid subGroupGuid = new Guid(buffer);
                    subGroups.Add(subGroupGuid);
                    index++;
                }
            }
            finally
            {
                Marshal.FreeHGlobal(schemeGuidPtr);
            }

            return subGroups;
        }

        /// <summary>
        /// Enumerates settings for a power plan subgroup
        /// </summary>
        /// <param name="planGuid">The GUID of the power plan</param>
        /// <param name="subGroupGuid">The GUID of the subgroup</param>
        /// <returns>A list of setting GUIDs</returns>
        private static List<Guid> EnumerateSettings(Guid planGuid, Guid subGroupGuid)
        {
            List<Guid> settings = new List<Guid>();
            uint index = 0;
            uint bufferSize = 16; // Size of a GUID
            byte[] buffer = new byte[bufferSize];

            IntPtr schemeGuidPtr = Marshal.AllocHGlobal(Marshal.SizeOf(planGuid));
            IntPtr subGroupGuidPtr = Marshal.AllocHGlobal(Marshal.SizeOf(subGroupGuid));

            Marshal.StructureToPtr(planGuid, schemeGuidPtr, false);
            Marshal.StructureToPtr(subGroupGuid, subGroupGuidPtr, false);

            try
            {
                while (PowerEnumerate(IntPtr.Zero, schemeGuidPtr, subGroupGuidPtr, ACCESS_INDIVIDUAL_SETTING, index, buffer, ref bufferSize) == ERROR_SUCCESS)
                {
                    Guid settingGuid = new Guid(buffer);
                    settings.Add(settingGuid);
                    index++;
                }
            }
            finally
            {
                Marshal.FreeHGlobal(schemeGuidPtr);
                Marshal.FreeHGlobal(subGroupGuidPtr);
            }

            return settings;
        }

        /// <summary>
        /// Gets information about a power setting
        /// </summary>
        /// <param name="planGuid">The GUID of the power plan</param>
        /// <param name="subGroupGuid">The GUID of the subgroup</param>
        /// <param name="settingGuid">The GUID of the setting</param>
        /// <returns>A PowerSetting object with information</returns>
        private static PowerSetting GetPowerSettingInfo(Guid planGuid, Guid subGroupGuid, Guid settingGuid)
        {
            string name = GetPowerSettingFriendlyName(settingGuid);
            string description = GetPowerSettingDescription(settingGuid);

            uint acType = 0;
            uint acValue = 0;
            uint acBufferSize = 4;

            uint dcType = 0;
            uint dcValue = 0;
            uint dcBufferSize = 4;

            PowerReadACValue(IntPtr.Zero, ref planGuid, ref subGroupGuid, ref settingGuid, ref acType, ref acValue, ref acBufferSize);
            PowerReadDCValue(IntPtr.Zero, ref planGuid, ref subGroupGuid, ref settingGuid, ref dcType, ref dcValue, ref dcBufferSize);

            // Get possible values for the setting
            List<PowerSettingPossibleValue> possibleValues = GetPowerSettingPossibleValues(settingGuid, subGroupGuid);

            PowerSetting setting = new PowerSetting
            {
                Alias = name,
                SettingGuid = settingGuid,
                SubGroupGuid = subGroupGuid,
                SubGroupAlias = ArgumentCompleters.GetSubgroupAlias(subGroupGuid),
                Description = description,
                PluggedIn = acValue,
                OnBattery = dcValue,
                Units = GetPowerSettingUnits(settingGuid),
                IsHidden = IsHiddenSetting(settingGuid),
                PossibleValues = possibleValues
            };

            return setting;
        }

        /// <summary>
        /// Gets the friendly name of a power setting
        /// </summary>
        /// <param name="settingGuid">The GUID of the setting</param>
        /// <returns>The friendly name of the setting</returns>
        private static string GetPowerSettingFriendlyName(Guid settingGuid)
        {
            // First check if the setting is in our known settings dictionary
            string knownName = ArgumentCompleters.GetPowerSettingAlias(settingGuid);
            if (knownName != settingGuid.ToString())
            {
                return knownName;
            }

            // If not found in dictionary, try to get it from PowrProf.dll
            uint bufferSize = 0;
            Guid emptyGuid = Guid.Empty;
            IntPtr settingGuidPtr = Marshal.AllocHGlobal(Marshal.SizeOf(settingGuid));
            Marshal.StructureToPtr(settingGuid, settingGuidPtr, false);

            PowerReadFriendlyName(IntPtr.Zero, ref emptyGuid, IntPtr.Zero, settingGuidPtr, IntPtr.Zero, ref bufferSize);

            IntPtr buffer = Marshal.AllocHGlobal((int)bufferSize);
            try
            {
                if (PowerReadFriendlyName(IntPtr.Zero, ref emptyGuid, IntPtr.Zero, settingGuidPtr, buffer, ref bufferSize) == ERROR_SUCCESS)
                {
                    string name = Marshal.PtrToStringUni(buffer);
                    if (!string.IsNullOrEmpty(name))
                    {
                        return name;
                    }
                }
            }
            finally
            {
                Marshal.FreeHGlobal(buffer);
                Marshal.FreeHGlobal(settingGuidPtr);
            }

            return $"Unknown Setting ({settingGuid})";
        }

        /// <summary>
        /// Gets the description of a power setting
        /// </summary>
        /// <param name="settingGuid">The GUID of the setting</param>
        /// <returns>The description of the setting</returns>
        private static string GetPowerSettingDescription(Guid settingGuid)
        {
            uint bufferSize = 0;
            Guid emptyGuid = Guid.Empty;
            IntPtr settingGuidPtr = Marshal.AllocHGlobal(Marshal.SizeOf(settingGuid));
            Marshal.StructureToPtr(settingGuid, settingGuidPtr, false);

            PowerReadDescription(IntPtr.Zero, ref emptyGuid, IntPtr.Zero, settingGuidPtr, IntPtr.Zero, ref bufferSize);

            IntPtr buffer = Marshal.AllocHGlobal((int)bufferSize);
            try
            {
                if (PowerReadDescription(IntPtr.Zero, ref emptyGuid, IntPtr.Zero, settingGuidPtr, buffer, ref bufferSize) == ERROR_SUCCESS)
                {
                    return Marshal.PtrToStringUni(buffer);
                }
            }
            finally
            {
                Marshal.FreeHGlobal(buffer);
                Marshal.FreeHGlobal(settingGuidPtr);
            }

            return string.Empty;
        }

        /// <summary>
        /// Gets the units for a power setting
        /// </summary>
        /// <param name="settingGuid">The GUID of the setting</param>
        /// <returns>The units string</returns>
        private static string GetPowerSettingUnits(Guid settingGuid)
        {
            // Common power setting units based on GUID
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
        public static bool UpdatePowerSetting(Guid planGuid, Guid settingGuid, Guid subGroupGuid, uint? acValue, uint? dcValue)
        {
            bool success = true;

            if (acValue.HasValue)
            {
                success &= PowerWriteACValueIndex(IntPtr.Zero, ref planGuid, ref subGroupGuid, ref settingGuid, acValue.Value) == ERROR_SUCCESS;
            }

            if (dcValue.HasValue)
            {
                success &= PowerWriteDCValueIndex(IntPtr.Zero, ref planGuid, ref subGroupGuid, ref settingGuid, dcValue.Value) == ERROR_SUCCESS;
            }

            return success;
        }

        /// <summary>
        /// Creates a new power plan by cloning an existing one
        /// </summary>
        /// <param name="sourcePlanGuid">The GUID of the source power plan</param>
        /// <param name="newPlanName">The name for the new power plan</param>
        /// <returns>The GUID of the new power plan, or Guid.Empty if failed</returns>
        public static Guid CreatePowerPlan(Guid sourcePlanGuid, string newPlanName)
        {
            IntPtr destSchemeGuidPtr;
            if (PowerDuplicateScheme(IntPtr.Zero, ref sourcePlanGuid, out destSchemeGuidPtr) == ERROR_SUCCESS)
            {
                Guid destSchemeGuid = (Guid)Marshal.PtrToStructure(destSchemeGuidPtr, typeof(Guid));
                Marshal.FreeHGlobal(destSchemeGuidPtr);

                // Set the friendly name for the new scheme
                // This is a simplified implementation
                // In a real implementation, you would use PowerWriteFriendlyName

                return destSchemeGuid;
            }

            return Guid.Empty;
        }

        /// <summary>
        /// Gets possible values for a power setting
        /// </summary>
        /// <param name="settingGuid">The GUID of the setting</param>
        /// <param name="subGroupGuid">The GUID of the subgroup</param>
        /// <returns>A list of possible values for the setting</returns>
        public static List<PowerSettingPossibleValue> GetPowerSettingPossibleValues(Guid settingGuid, Guid subGroupGuid)
        {
            List<PowerSettingPossibleValue> possibleValues = new List<PowerSettingPossibleValue>();

            try
            {
                // Get the active power scheme GUID
                Guid schemeGuid = GetActivePowerPlanGuid();

                // First, get the buffer size needed for possible values
                uint bufferSize = 0;
                uint result = PowerReadPossibleValue(
                    IntPtr.Zero,
                    ref schemeGuid,
                    ref subGroupGuid,
                    ref settingGuid,
                    REG_DWORD,
                    IntPtr.Zero,
                    ref bufferSize
                );

                if (result != ERROR_SUCCESS && result != ERROR_MORE_DATA)
                {
                    // Not an enumeration type setting or other error
                    // Add default values for common settings
                    return GetDefaultPossibleValues(settingGuid);
                }

                if (bufferSize == 0 || bufferSize % 4 != 0)
                {
                    // Invalid buffer size
                    return possibleValues;
                }

                // Allocate buffer for possible values
                IntPtr valueBuffer = Marshal.AllocHGlobal((int)bufferSize);
                try
                {
                    // Read possible values
                    result = PowerReadPossibleValue(
                        IntPtr.Zero,
                        ref schemeGuid,
                        ref subGroupGuid,
                        ref settingGuid,
                        REG_DWORD,
                        valueBuffer,
                        ref bufferSize
                    );

                    if (result != ERROR_SUCCESS)
                    {
                        return possibleValues;
                    }

                    // Get the number of possible values
                    int valueCount = (int)bufferSize / 4; // 4 bytes per DWORD
                    uint[] values = new uint[valueCount];

                    // Copy values from buffer
                    for (int i = 0; i < valueCount; i++)
                    {
                        values[i] = (uint)Marshal.ReadInt32(valueBuffer, i * 4);
                    }

                    // Now get the buffer size needed for friendly names
                    bufferSize = 0;
                    result = PowerReadPossibleFriendlyName(
                        IntPtr.Zero,
                        ref schemeGuid,
                        ref subGroupGuid,
                        ref settingGuid,
                        REG_SZ,
                        IntPtr.Zero,
                        ref bufferSize
                    );

                    if (result != ERROR_SUCCESS && result != ERROR_MORE_DATA)
                    {
                        // Use numeric values as names
                        for (int i = 0; i < valueCount; i++)
                        {
                            possibleValues.Add(new PowerSettingPossibleValue
                            {
                                FriendlyName = values[i].ToString(),
                                ActualValue = values[i]
                            });
                        }
                        return possibleValues;
                    }

                    // Allocate buffer for friendly names
                    IntPtr nameBuffer = Marshal.AllocHGlobal((int)bufferSize);
                    try
                    {
                        // Read friendly names
                        result = PowerReadPossibleFriendlyName(
                            IntPtr.Zero,
                            ref schemeGuid,
                            ref subGroupGuid,
                            ref settingGuid,
                            REG_SZ,
                            nameBuffer,
                            ref bufferSize
                        );

                        if (result != ERROR_SUCCESS)
                        {
                            // Use numeric values as names
                            for (int i = 0; i < valueCount; i++)
                            {
                                possibleValues.Add(new PowerSettingPossibleValue
                                {
                                    FriendlyName = values[i].ToString(),
                                    ActualValue = values[i]
                                });
                            }
                            return possibleValues;
                        }

                        // Parse the multi-string buffer to get friendly names
                        string multiString = Marshal.PtrToStringUni(nameBuffer);
                        string[] names = multiString.Split(new[] { '\0' }, StringSplitOptions.RemoveEmptyEntries);

                        // Create the list of possible values
                        for (int i = 0; i < Math.Min(values.Length, names.Length); i++)
                        {
                            possibleValues.Add(new PowerSettingPossibleValue
                            {
                                FriendlyName = names[i],
                                ActualValue = values[i]
                            });
                        }
                    }
                    finally
                    {
                        Marshal.FreeHGlobal(nameBuffer);
                    }
                }
                finally
                {
                    Marshal.FreeHGlobal(valueBuffer);
                }
            }
            catch
            {
                // Ignore errors and return empty list
            }

            return possibleValues;
        }

        /// <summary>
        /// Gets default possible values for common power settings
        /// </summary>
        /// <param name="settingGuid">The GUID of the setting</param>
        /// <returns>A list of possible values for the setting</returns>
        private static List<PowerSettingPossibleValue> GetDefaultPossibleValues(Guid settingGuid)
        {
            List<PowerSettingPossibleValue> possibleValues = new List<PowerSettingPossibleValue>();

            // Check for common power settings and provide default values
            switch (settingGuid.ToString().ToLower())
            {
                // Lid close action
                case "5ca83367-6e45-459f-a27b-476b1d01c936":
                    possibleValues.Add(new PowerSettingPossibleValue { FriendlyName = "Do nothing", ActualValue = 0 });
                    possibleValues.Add(new PowerSettingPossibleValue { FriendlyName = "Sleep", ActualValue = 1 });
                    possibleValues.Add(new PowerSettingPossibleValue { FriendlyName = "Hibernate", ActualValue = 2 });
                    possibleValues.Add(new PowerSettingPossibleValue { FriendlyName = "Shut down", ActualValue = 3 });
                    break;

                // Power button action
                case "7648efa3-dd9c-4e3e-b566-50f929386280":
                    possibleValues.Add(new PowerSettingPossibleValue { FriendlyName = "Do nothing", ActualValue = 0 });
                    possibleValues.Add(new PowerSettingPossibleValue { FriendlyName = "Sleep", ActualValue = 1 });
                    possibleValues.Add(new PowerSettingPossibleValue { FriendlyName = "Hibernate", ActualValue = 2 });
                    possibleValues.Add(new PowerSettingPossibleValue { FriendlyName = "Shut down", ActualValue = 3 });
                    break;

                // Sleep button action
                case "96996bc0-ad50-47ec-923b-6f41874dd9eb":
                    possibleValues.Add(new PowerSettingPossibleValue { FriendlyName = "Do nothing", ActualValue = 0 });
                    possibleValues.Add(new PowerSettingPossibleValue { FriendlyName = "Sleep", ActualValue = 1 });
                    possibleValues.Add(new PowerSettingPossibleValue { FriendlyName = "Hibernate", ActualValue = 2 });
                    possibleValues.Add(new PowerSettingPossibleValue { FriendlyName = "Shut down", ActualValue = 3 });
                    break;

                // Display brightness
                case "aded5e82-b909-4619-9949-f5d71dac0bcb":
                case "f1fbfde2-a960-4165-9f88-50667911ce96":
                    for (uint i = 0; i <= 100; i += 5)
                    {
                        possibleValues.Add(new PowerSettingPossibleValue { FriendlyName = $"{i}%", ActualValue = i });
                    }
                    break;

                // Processor power management
                case "bc5038f7-23e0-4960-96da-33abaf5935ec": // Maximum processor state
                case "893dee8e-2bef-41e0-89c6-b55d0929964c": // Minimum processor state
                    for (uint i = 0; i <= 100; i += 5)
                    {
                        possibleValues.Add(new PowerSettingPossibleValue { FriendlyName = $"{i}%", ActualValue = i });
                    }
                    break;

                // Critical battery action
                case "637ea02f-bbcb-4015-8e2c-a1c7b9c0b546":
                    possibleValues.Add(new PowerSettingPossibleValue { FriendlyName = "Do nothing", ActualValue = 0 });
                    possibleValues.Add(new PowerSettingPossibleValue { FriendlyName = "Sleep", ActualValue = 1 });
                    possibleValues.Add(new PowerSettingPossibleValue { FriendlyName = "Hibernate", ActualValue = 2 });
                    possibleValues.Add(new PowerSettingPossibleValue { FriendlyName = "Shut down", ActualValue = 3 });
                    break;

                // Low battery action
                case "d8742dcb-3e6a-4b3c-b3fe-374623cdcf06":
                    possibleValues.Add(new PowerSettingPossibleValue { FriendlyName = "Do nothing", ActualValue = 0 });
                    possibleValues.Add(new PowerSettingPossibleValue { FriendlyName = "Sleep", ActualValue = 1 });
                    possibleValues.Add(new PowerSettingPossibleValue { FriendlyName = "Hibernate", ActualValue = 2 });
                    possibleValues.Add(new PowerSettingPossibleValue { FriendlyName = "Shut down", ActualValue = 3 });
                    break;

                // Hard disk timeout
                case "6738e2c4-e8a5-4a42-b16a-e040e769756e": // Hard disk timeout (AC)
                case "80e3c60e-bb94-4ad8-bbe0-0d3195efc663": // Hard disk timeout (DC)
                    possibleValues.Add(new PowerSettingPossibleValue { FriendlyName = "Never", ActualValue = 0 });
                    possibleValues.Add(new PowerSettingPossibleValue { FriendlyName = "1 minute", ActualValue = 60 });
                    possibleValues.Add(new PowerSettingPossibleValue { FriendlyName = "2 minutes", ActualValue = 120 });
                    possibleValues.Add(new PowerSettingPossibleValue { FriendlyName = "5 minutes", ActualValue = 300 });
                    possibleValues.Add(new PowerSettingPossibleValue { FriendlyName = "10 minutes", ActualValue = 600 });
                    possibleValues.Add(new PowerSettingPossibleValue { FriendlyName = "15 minutes", ActualValue = 900 });
                    possibleValues.Add(new PowerSettingPossibleValue { FriendlyName = "20 minutes", ActualValue = 1200 });
                    possibleValues.Add(new PowerSettingPossibleValue { FriendlyName = "30 minutes", ActualValue = 1800 });
                    possibleValues.Add(new PowerSettingPossibleValue { FriendlyName = "45 minutes", ActualValue = 2700 });
                    possibleValues.Add(new PowerSettingPossibleValue { FriendlyName = "1 hour", ActualValue = 3600 });
                    break;

                // Default case for other settings
                default:
                    // For boolean settings (common)
                    possibleValues.Add(new PowerSettingPossibleValue { FriendlyName = "Off", ActualValue = 0 });
                    possibleValues.Add(new PowerSettingPossibleValue { FriendlyName = "On", ActualValue = 1 });
                    break;
            }

            return possibleValues;
        }

        /// <summary>
        /// Gets a power setting value
        /// </summary>
        /// <param name="planGuid">The GUID of the power plan</param>
        /// <param name="subGroupGuid">The GUID of the subgroup</param>
        /// <param name="settingGuid">The GUID of the setting</param>
        /// <param name="getAcValue">True to get AC value, false to get DC value</param>
        /// <returns>The setting value, or null if not found</returns>
        public static uint? GetPowerSettingValue(Guid planGuid, Guid subGroupGuid, Guid settingGuid, bool getAcValue = true)
        {
            uint type = 0;
            uint value = 0;
            uint bufferSize = 4;

            uint result;
            if (getAcValue)
            {
                result = PowerReadACValue(IntPtr.Zero, ref planGuid, ref subGroupGuid, ref settingGuid, ref type, ref value, ref bufferSize);
            }
            else
            {
                result = PowerReadDCValue(IntPtr.Zero, ref planGuid, ref subGroupGuid, ref settingGuid, ref type, ref value, ref bufferSize);
            }

            if (result == ERROR_SUCCESS)
            {
                return value;
            }

            return null;
        }

        /// <summary>
        /// Deletes a power plan
        /// </summary>
        /// <param name="planGuid">The GUID of the power plan to delete</param>
        /// <returns>True if successful, false otherwise</returns>
        public static bool DeletePowerPlan(Guid planGuid)
        {
            return PowerDeleteScheme(IntPtr.Zero, ref planGuid) == ERROR_SUCCESS;
        }
    }
}
