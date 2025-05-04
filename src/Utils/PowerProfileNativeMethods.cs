using System;
using System.Runtime.InteropServices;

namespace PowerPlanTools.Utils
{
    /// <summary>
    /// Native methods for interacting with the Windows Power Management API.
    /// </summary>
    internal static class PowerProfileNativeMethods
    {
        // Constants
        public const uint ACCESS_SCHEME = 16;
        public const uint ACCESS_SUBGROUP = 17;
        public const uint ACCESS_INDIVIDUAL_SETTING = 18;
        
        // Value types
        public const uint REG_NONE = 0;
        public const uint REG_SZ = 1;
        public const uint REG_BINARY = 3;
        public const uint REG_DWORD = 4;
        
        // Error codes
        public const uint ERROR_SUCCESS = 0;
        public const uint ERROR_MORE_DATA = 234;
        
        /// <summary>
        /// Gets the active power scheme.
        /// </summary>
        [DllImport("PowrProf.dll", CharSet = CharSet.Unicode)]
        public static extern uint PowerGetActiveScheme(
            IntPtr UserRootPowerKey,
            out IntPtr ActivePolicyGuid
        );
        
        /// <summary>
        /// Opens a user power key.
        /// </summary>
        [DllImport("PowrProf.dll", CharSet = CharSet.Unicode)]
        public static extern uint PowerOpenUserPowerKey(
            out IntPtr phUserPowerKey,
            uint Access,
            bool fWritable
        );
        
        /// <summary>
        /// Opens a specific power scheme.
        /// </summary>
        [DllImport("PowrProf.dll", CharSet = CharSet.Unicode)]
        public static extern uint PowerOpenScheme(
            IntPtr RootPowerKey,
            ref Guid SchemeGuid,
            out IntPtr SchemeKey,
            uint Access,
            bool fWritable
        );
        
        /// <summary>
        /// Reads the AC value index for a power setting.
        /// </summary>
        [DllImport("PowrProf.dll", CharSet = CharSet.Unicode)]
        public static extern uint PowerReadACValueIndex(
            IntPtr RootPowerKey,
            ref Guid SchemeGuid,
            ref Guid SubGroupGuid,
            ref Guid PowerSettingGuid,
            out uint Value
        );
        
        /// <summary>
        /// Reads the DC value index for a power setting.
        /// </summary>
        [DllImport("PowrProf.dll", CharSet = CharSet.Unicode)]
        public static extern uint PowerReadDCValueIndex(
            IntPtr RootPowerKey,
            ref Guid SchemeGuid,
            ref Guid SubGroupGuid,
            ref Guid PowerSettingGuid,
            out uint Value
        );
        
        /// <summary>
        /// Writes the AC value index for a power setting.
        /// </summary>
        [DllImport("PowrProf.dll", CharSet = CharSet.Unicode)]
        public static extern uint PowerWriteACValueIndex(
            IntPtr RootPowerKey,
            ref Guid SchemeGuid,
            ref Guid SubGroupGuid,
            ref Guid PowerSettingGuid,
            uint Value
        );
        
        /// <summary>
        /// Writes the DC value index for a power setting.
        /// </summary>
        [DllImport("PowrProf.dll", CharSet = CharSet.Unicode)]
        public static extern uint PowerWriteDCValueIndex(
            IntPtr RootPowerKey,
            ref Guid SchemeGuid,
            ref Guid SubGroupGuid,
            ref Guid PowerSettingGuid,
            uint Value
        );
        
        /// <summary>
        /// Sets the active power scheme.
        /// </summary>
        [DllImport("PowrProf.dll", CharSet = CharSet.Unicode)]
        public static extern uint PowerSetActiveScheme(
            IntPtr UserRootPowerKey,
            ref Guid SchemeGuid
        );
        
        /// <summary>
        /// Reads the friendly name for a power setting.
        /// </summary>
        [DllImport("PowrProf.dll", CharSet = CharSet.Unicode)]
        public static extern uint PowerReadFriendlyName(
            IntPtr RootPowerKey,
            ref Guid SchemeGuid,
            ref Guid SubGroupGuid,
            ref Guid PowerSettingGuid,
            IntPtr Buffer,
            ref uint BufferSize
        );
        
        /// <summary>
        /// Reads the description for a power setting.
        /// </summary>
        [DllImport("PowrProf.dll", CharSet = CharSet.Unicode)]
        public static extern uint PowerReadDescription(
            IntPtr RootPowerKey,
            ref Guid SchemeGuid,
            ref Guid SubGroupGuid,
            ref Guid PowerSettingGuid,
            IntPtr Buffer,
            ref uint BufferSize
        );
        
        /// <summary>
        /// Reads possible values for a power setting.
        /// </summary>
        [DllImport("PowrProf.dll", CharSet = CharSet.Unicode)]
        public static extern uint PowerReadPossibleValue(
            IntPtr RootPowerKey,
            ref Guid SchemeGuid,
            ref Guid SubGroupGuid,
            ref Guid PowerSettingGuid,
            uint Type,
            IntPtr Buffer,
            ref uint BufferSize
        );
        
        /// <summary>
        /// Reads possible friendly names for a power setting.
        /// </summary>
        [DllImport("PowrProf.dll", CharSet = CharSet.Unicode)]
        public static extern uint PowerReadPossibleFriendlyName(
            IntPtr RootPowerKey,
            ref Guid SchemeGuid,
            ref Guid SubGroupGuid,
            ref Guid PowerSettingGuid,
            uint Type,
            IntPtr Buffer,
            ref uint BufferSize
        );
        
        /// <summary>
        /// Enumerates power schemes.
        /// </summary>
        [DllImport("PowrProf.dll", CharSet = CharSet.Unicode)]
        public static extern uint PowerEnumerate(
            IntPtr RootPowerKey,
            IntPtr SchemeGuid,
            IntPtr SubGroupGuid,
            uint AccessFlags,
            uint Index,
            IntPtr Buffer,
            ref uint BufferSize
        );
        
        /// <summary>
        /// Closes a power key.
        /// </summary>
        [DllImport("PowrProf.dll", CharSet = CharSet.Unicode)]
        public static extern uint PowerCloseKey(
            IntPtr PowerKey
        );
    }
}
