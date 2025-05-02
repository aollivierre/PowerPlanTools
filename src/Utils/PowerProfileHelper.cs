using System;
using System.Collections.Generic;
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

            PowerSetting setting = new PowerSetting
            {
                Alias = name,
                SettingGuid = settingGuid,
                SubGroupGuid = subGroupGuid,
                Description = description,
                PluggedIn = acValue,
                OnBattery = dcValue,
                Units = GetPowerSettingUnits(settingGuid),
                IsHidden = IsHiddenSetting(settingGuid)
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
                    return Marshal.PtrToStringUni(buffer);
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
            // This is a simplified implementation
            // In a real implementation, you would use the PowerReadSettingAttributes function
            // or other means to get the units
            return string.Empty;
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
