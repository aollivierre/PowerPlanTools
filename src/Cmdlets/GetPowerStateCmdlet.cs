using System;
using System.Management.Automation;
using PowerPlanTools.Models;
using PowerPlanTools.Utils;

namespace PowerPlanTools.Cmdlets
{
    /// <summary>
    /// <para type="synopsis">Gets the current power state settings.</para>
    /// <para type="description">The Get-PowerState cmdlet retrieves the current state of various power settings like Connected Standby, S0 Low Power Idle, Fast Startup, etc.</para>
    /// <example>
    ///     <para>Get all power state settings</para>
    ///     <code>Get-PowerState</code>
    /// </example>
    /// <example>
    ///     <para>Check if Connected Standby is enabled</para>
    ///     <code>Get-PowerState | Select-Object ConnectedStandby</code>
    /// </example>
    /// </summary>
    [Cmdlet(VerbsCommon.Get, "PowerState")]
    [OutputType(typeof(PowerStateInfo))]
    public class GetPowerStateCmdlet : PSCmdlet
    {
        /// <summary>
        /// <para type="description">Gets or sets whether to use WMI instead of PowrProf.dll.</para>
        /// </summary>
        [Parameter]
        public SwitchParameter UseWmi { get; set; }

        /// <summary>
        /// Processes the cmdlet.
        /// </summary>
        protected override void ProcessRecord()
        {
            try
            {
                // Get the active power plan
                Guid activePlanGuid = UseWmi ? WmiHelper.GetActivePowerPlanGuid() : PowerProfileHelper.GetActivePowerPlanGuid();
                
                // Create a new PowerStateInfo object
                PowerStateInfo stateInfo = new PowerStateInfo();
                
                // Get Connected Standby state
                stateInfo.ConnectedStandby = GetConnectedStandbyState(activePlanGuid);
                
                // Get Fast Startup state
                stateInfo.FastStartup = GetFastStartupState();
                
                // Get S0 Low Power Idle state
                stateInfo.S0LowPowerIdle = GetS0LowPowerIdleState(activePlanGuid);
                
                // Get Hibernate state
                stateInfo.Hibernate = GetHibernateState();
                
                // Get Hybrid Sleep state
                stateInfo.HybridSleep = GetHybridSleepState(activePlanGuid);
                
                // Write the output
                WriteObject(stateInfo);
            }
            catch (Exception ex)
            {
                WriteError(new ErrorRecord(ex, "GetPowerStateError", ErrorCategory.NotSpecified, null));
            }
        }

        /// <summary>
        /// Gets the Connected Standby state.
        /// </summary>
        /// <param name="planGuid">The active power plan GUID.</param>
        /// <returns>True if enabled, false if disabled.</returns>
        private bool GetConnectedStandbyState(Guid planGuid)
        {
            try
            {
                // Connected Standby is controlled by the "Disconnected Standby Mode" setting
                Guid settingGuid = new Guid("543e0f88-611b-4eab-bf9d-c9c38790c55e");
                Guid subGroupGuid = new Guid("5d3e9a59-e9d5-4b00-a6bd-ff34ff516548"); // System settings subgroup
                
                // Get the current setting value (AC)
                uint? value = null;
                if (UseWmi)
                {
                    var settings = WmiHelper.GetPowerSettings(planGuid, true);
                    var setting = settings.Find(s => s.SettingGuid == settingGuid && s.SubGroupGuid == subGroupGuid);
                    if (setting != null && setting.PluggedIn != null)
                    {
                        value = Convert.ToUInt32(setting.PluggedIn);
                    }
                }
                else
                {
                    value = PowerProfileHelper.GetPowerSettingValue(planGuid, subGroupGuid, settingGuid, true);
                }
                
                // 0 = Disabled, 1 = Enabled
                return value.HasValue && value.Value == 1;
            }
            catch
            {
                // If there's an error, assume it's not enabled
                return false;
            }
        }

        /// <summary>
        /// Gets the Fast Startup state.
        /// </summary>
        /// <returns>True if enabled, false if disabled.</returns>
        private bool GetFastStartupState()
        {
            try
            {
                // Fast Startup is controlled via the registry
                System.Diagnostics.Process process = new System.Diagnostics.Process();
                process.StartInfo.FileName = "cmd.exe";
                process.StartInfo.Arguments = "/c powercfg /getacvalueindex SCHEME_CURRENT SUB_SLEEP HIBERNATEPOWERDOWN";
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.CreateNoWindow = true;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError = true;
                process.Start();
                string output = process.StandardOutput.ReadToEnd();
                process.WaitForExit();
                
                // Parse the output to get the value
                if (process.ExitCode == 0 && !string.IsNullOrEmpty(output))
                {
                    // The output contains the index value (0 = Disabled, 1 = Enabled)
                    if (uint.TryParse(output.Trim(), out uint value))
                    {
                        return value == 1;
                    }
                }
                
                return false;
            }
            catch
            {
                // If there's an error, assume it's not enabled
                return false;
            }
        }

        /// <summary>
        /// Gets the S0 Low Power Idle state.
        /// </summary>
        /// <param name="planGuid">The active power plan GUID.</param>
        /// <returns>True if enabled, false if disabled.</returns>
        private bool GetS0LowPowerIdleState(Guid planGuid)
        {
            try
            {
                // S0 Low Power Idle is controlled by the "Low Power S0 Idle Capability" setting
                Guid settingGuid = new Guid("4e4450b3-6179-4e91-b8f1-5bb9938f81a1");
                Guid subGroupGuid = new Guid("5d3e9a59-e9d5-4b00-a6bd-ff34ff516548"); // System settings subgroup
                
                // Get the current setting value (AC)
                uint? value = null;
                if (UseWmi)
                {
                    var settings = WmiHelper.GetPowerSettings(planGuid, true);
                    var setting = settings.Find(s => s.SettingGuid == settingGuid && s.SubGroupGuid == subGroupGuid);
                    if (setting != null && setting.PluggedIn != null)
                    {
                        value = Convert.ToUInt32(setting.PluggedIn);
                    }
                }
                else
                {
                    value = PowerProfileHelper.GetPowerSettingValue(planGuid, subGroupGuid, settingGuid, true);
                }
                
                // 0 = Disabled, 1 = Enabled
                return value.HasValue && value.Value == 1;
            }
            catch
            {
                // If there's an error, assume it's not enabled
                return false;
            }
        }

        /// <summary>
        /// Gets the Hibernate state.
        /// </summary>
        /// <returns>True if enabled, false if disabled.</returns>
        private bool GetHibernateState()
        {
            try
            {
                // Check if hibernate is enabled
                System.Diagnostics.Process process = new System.Diagnostics.Process();
                process.StartInfo.FileName = "cmd.exe";
                process.StartInfo.Arguments = "/c powercfg /a | findstr /c:\"Hibernation\"";
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.CreateNoWindow = true;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError = true;
                process.Start();
                string output = process.StandardOutput.ReadToEnd();
                process.WaitForExit();
                
                // If hibernate is enabled, the output will contain "Hibernation: Available"
                return output.Contains("Hibernation: Available");
            }
            catch
            {
                // If there's an error, assume it's not enabled
                return false;
            }
        }

        /// <summary>
        /// Gets the Hybrid Sleep state.
        /// </summary>
        /// <param name="planGuid">The active power plan GUID.</param>
        /// <returns>True if enabled, false if disabled.</returns>
        private bool GetHybridSleepState(Guid planGuid)
        {
            try
            {
                // Hybrid Sleep is controlled by the "Allow hybrid sleep" setting
                Guid settingGuid = new Guid("bd3b718a-0680-4d9d-8ab2-e1d2b4ac806d");
                Guid subGroupGuid = new Guid("238c9fa8-0aad-41ed-83f4-97be242c8f20"); // Sleep subgroup
                
                // Get the current setting value (AC)
                uint? value = null;
                if (UseWmi)
                {
                    var settings = WmiHelper.GetPowerSettings(planGuid, true);
                    var setting = settings.Find(s => s.SettingGuid == settingGuid && s.SubGroupGuid == subGroupGuid);
                    if (setting != null && setting.PluggedIn != null)
                    {
                        value = Convert.ToUInt32(setting.PluggedIn);
                    }
                }
                else
                {
                    value = PowerProfileHelper.GetPowerSettingValue(planGuid, subGroupGuid, settingGuid, true);
                }
                
                // 0 = Disabled, 1 = Enabled
                return value.HasValue && value.Value == 1;
            }
            catch
            {
                // If there's an error, assume it's not enabled
                return false;
            }
        }
    }
}
