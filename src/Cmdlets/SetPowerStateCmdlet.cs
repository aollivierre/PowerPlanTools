using System;
using System.Management.Automation;
using PowerPlanTools.Models;
using PowerPlanTools.Utils;

namespace PowerPlanTools.Cmdlets
{
    /// <summary>
    /// <para type="synopsis">Enables or disables power state settings.</para>
    /// <para type="description">The Set-PowerState cmdlet enables or disables various power state settings like Connected Standby, S0 Low Power Idle, Fast Startup, etc.</para>
    /// <example>
    ///     <para>Enable Connected Standby</para>
    ///     <code>Set-PowerState -ConnectedStandby $true</code>
    /// </example>
    /// <example>
    ///     <para>Disable Fast Startup</para>
    ///     <code>Set-PowerState -FastStartup $false</code>
    /// </example>
    /// <example>
    ///     <para>Enable S0 Low Power Idle</para>
    ///     <code>Set-PowerState -S0LowPowerIdle $true</code>
    /// </example>
    /// </summary>
    [Cmdlet(VerbsCommon.Set, "PowerState", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
    public class SetPowerStateCmdlet : PSCmdlet
    {
        /// <summary>
        /// <para type="description">Gets or sets whether Connected Standby (Modern Standby) is enabled.</para>
        /// </summary>
        [Parameter(ParameterSetName = "ConnectedStandby")]
        public bool? ConnectedStandby { get; set; }

        /// <summary>
        /// <para type="description">Gets or sets whether Fast Startup is enabled.</para>
        /// </summary>
        [Parameter(ParameterSetName = "FastStartup")]
        public bool? FastStartup { get; set; }

        /// <summary>
        /// <para type="description">Gets or sets whether S0 Low Power Idle is enabled.</para>
        /// </summary>
        [Parameter(ParameterSetName = "S0LowPowerIdle")]
        public bool? S0LowPowerIdle { get; set; }

        /// <summary>
        /// <para type="description">Gets or sets whether Hibernate is enabled.</para>
        /// </summary>
        [Parameter(ParameterSetName = "Hibernate")]
        public bool? Hibernate { get; set; }

        /// <summary>
        /// <para type="description">Gets or sets whether Hybrid Sleep is enabled.</para>
        /// </summary>
        [Parameter(ParameterSetName = "HybridSleep")]
        public bool? HybridSleep { get; set; }

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
                // Handle Connected Standby
                if (ConnectedStandby.HasValue)
                {
                    string action = ConnectedStandby.Value ? "Enable" : "Disable";
                    if (ShouldProcess($"Connected Standby", action))
                    {
                        bool success = SetConnectedStandby(ConnectedStandby.Value);
                        if (!success)
                        {
                            WriteError(new ErrorRecord(
                                new InvalidOperationException($"Failed to {action.ToLower()} Connected Standby."),
                                "SetConnectedStandbyFailed",
                                ErrorCategory.InvalidOperation,
                                ConnectedStandby.Value));
                            return;
                        }
                        LoggingHelper.LogVerbose(this, $"Successfully {action.ToLower()}d Connected Standby.");
                    }
                }

                // Handle Fast Startup
                if (FastStartup.HasValue)
                {
                    string action = FastStartup.Value ? "Enable" : "Disable";
                    if (ShouldProcess($"Fast Startup", action))
                    {
                        bool success = SetFastStartup(FastStartup.Value);
                        if (!success)
                        {
                            WriteError(new ErrorRecord(
                                new InvalidOperationException($"Failed to {action.ToLower()} Fast Startup."),
                                "SetFastStartupFailed",
                                ErrorCategory.InvalidOperation,
                                FastStartup.Value));
                            return;
                        }
                        LoggingHelper.LogVerbose(this, $"Successfully {action.ToLower()}d Fast Startup.");
                    }
                }

                // Handle S0 Low Power Idle
                if (S0LowPowerIdle.HasValue)
                {
                    string action = S0LowPowerIdle.Value ? "Enable" : "Disable";
                    if (ShouldProcess($"S0 Low Power Idle", action))
                    {
                        bool success = SetS0LowPowerIdle(S0LowPowerIdle.Value);
                        if (!success)
                        {
                            WriteError(new ErrorRecord(
                                new InvalidOperationException($"Failed to {action.ToLower()} S0 Low Power Idle."),
                                "SetS0LowPowerIdleFailed",
                                ErrorCategory.InvalidOperation,
                                S0LowPowerIdle.Value));
                            return;
                        }
                        LoggingHelper.LogVerbose(this, $"Successfully {action.ToLower()}d S0 Low Power Idle.");
                    }
                }

                // Handle Hibernate
                if (Hibernate.HasValue)
                {
                    string action = Hibernate.Value ? "Enable" : "Disable";
                    if (ShouldProcess($"Hibernate", action))
                    {
                        bool success = SetHibernate(Hibernate.Value);
                        if (!success)
                        {
                            WriteError(new ErrorRecord(
                                new InvalidOperationException($"Failed to {action.ToLower()} Hibernate."),
                                "SetHibernateFailed",
                                ErrorCategory.InvalidOperation,
                                Hibernate.Value));
                            return;
                        }
                        LoggingHelper.LogVerbose(this, $"Successfully {action.ToLower()}d Hibernate.");
                    }
                }

                // Handle Hybrid Sleep
                if (HybridSleep.HasValue)
                {
                    string action = HybridSleep.Value ? "Enable" : "Disable";
                    if (ShouldProcess($"Hybrid Sleep", action))
                    {
                        bool success = SetHybridSleep(HybridSleep.Value);
                        if (!success)
                        {
                            WriteError(new ErrorRecord(
                                new InvalidOperationException($"Failed to {action.ToLower()} Hybrid Sleep."),
                                "SetHybridSleepFailed",
                                ErrorCategory.InvalidOperation,
                                HybridSleep.Value));
                            return;
                        }
                        LoggingHelper.LogVerbose(this, $"Successfully {action.ToLower()}d Hybrid Sleep.");
                    }
                }
            }
            catch (Exception ex)
            {
                WriteError(new ErrorRecord(ex, "SetPowerStateError", ErrorCategory.NotSpecified, null));
            }
        }

        /// <summary>
        /// Sets the Connected Standby state.
        /// </summary>
        /// <param name="enable">Whether to enable or disable Connected Standby.</param>
        /// <returns>True if successful, false otherwise.</returns>
        private bool SetConnectedStandby(bool enable)
        {
            // Get the active power plan
            Guid activePlanGuid = UseWmi ? WmiHelper.GetActivePowerPlanGuid() : PowerProfileHelper.GetActivePowerPlanGuid();
            
            // Connected Standby is controlled by the "Disconnected Standby Mode" setting
            Guid settingGuid = new Guid("543e0f88-611b-4eab-bf9d-c9c38790c55e");
            Guid subGroupGuid = new Guid("5d3e9a59-e9d5-4b00-a6bd-ff34ff516548"); // System settings subgroup
            
            // 0 = Disabled, 1 = Enabled
            uint value = enable ? 1u : 0u;
            
            // Update the setting for both AC and DC
            if (UseWmi)
            {
                return WmiHelper.UpdatePowerSetting(activePlanGuid, settingGuid, subGroupGuid, value, value);
            }
            else
            {
                return PowerProfileHelper.UpdatePowerSetting(activePlanGuid, settingGuid, subGroupGuid, value, value);
            }
        }

        /// <summary>
        /// Sets the Fast Startup state.
        /// </summary>
        /// <param name="enable">Whether to enable or disable Fast Startup.</param>
        /// <returns>True if successful, false otherwise.</returns>
        private bool SetFastStartup(bool enable)
        {
            try
            {
                // Fast Startup is controlled via the registry
                string command = enable ? 
                    "powercfg /setacvalueindex SCHEME_CURRENT SUB_SLEEP HIBERNATEPOWERDOWN 1" : 
                    "powercfg /setacvalueindex SCHEME_CURRENT SUB_SLEEP HIBERNATEPOWERDOWN 0";
                
                // Execute the command
                System.Diagnostics.Process process = new System.Diagnostics.Process();
                process.StartInfo.FileName = "cmd.exe";
                process.StartInfo.Arguments = $"/c {command}";
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.CreateNoWindow = true;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError = true;
                process.Start();
                process.WaitForExit();
                
                return process.ExitCode == 0;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Sets the S0 Low Power Idle state.
        /// </summary>
        /// <param name="enable">Whether to enable or disable S0 Low Power Idle.</param>
        /// <returns>True if successful, false otherwise.</returns>
        private bool SetS0LowPowerIdle(bool enable)
        {
            // Get the active power plan
            Guid activePlanGuid = UseWmi ? WmiHelper.GetActivePowerPlanGuid() : PowerProfileHelper.GetActivePowerPlanGuid();
            
            // S0 Low Power Idle is controlled by the "Low Power S0 Idle Capability" setting
            Guid settingGuid = new Guid("4e4450b3-6179-4e91-b8f1-5bb9938f81a1");
            Guid subGroupGuid = new Guid("5d3e9a59-e9d5-4b00-a6bd-ff34ff516548"); // System settings subgroup
            
            // 0 = Disabled, 1 = Enabled
            uint value = enable ? 1u : 0u;
            
            // Update the setting for both AC and DC
            if (UseWmi)
            {
                return WmiHelper.UpdatePowerSetting(activePlanGuid, settingGuid, subGroupGuid, value, value);
            }
            else
            {
                return PowerProfileHelper.UpdatePowerSetting(activePlanGuid, settingGuid, subGroupGuid, value, value);
            }
        }

        /// <summary>
        /// Sets the Hibernate state.
        /// </summary>
        /// <param name="enable">Whether to enable or disable Hibernate.</param>
        /// <returns>True if successful, false otherwise.</returns>
        private bool SetHibernate(bool enable)
        {
            try
            {
                // Hibernate is controlled via powercfg
                string command = enable ? "powercfg /hibernate on" : "powercfg /hibernate off";
                
                // Execute the command
                System.Diagnostics.Process process = new System.Diagnostics.Process();
                process.StartInfo.FileName = "cmd.exe";
                process.StartInfo.Arguments = $"/c {command}";
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.CreateNoWindow = true;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError = true;
                process.Start();
                process.WaitForExit();
                
                return process.ExitCode == 0;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Sets the Hybrid Sleep state.
        /// </summary>
        /// <param name="enable">Whether to enable or disable Hybrid Sleep.</param>
        /// <returns>True if successful, false otherwise.</returns>
        private bool SetHybridSleep(bool enable)
        {
            // Get the active power plan
            Guid activePlanGuid = UseWmi ? WmiHelper.GetActivePowerPlanGuid() : PowerProfileHelper.GetActivePowerPlanGuid();
            
            // Hybrid Sleep is controlled by the "Allow hybrid sleep" setting
            Guid settingGuid = new Guid("bd3b718a-0680-4d9d-8ab2-e1d2b4ac806d");
            Guid subGroupGuid = new Guid("238c9fa8-0aad-41ed-83f4-97be242c8f20"); // Sleep subgroup
            
            // 0 = Disabled, 1 = Enabled
            uint value = enable ? 1u : 0u;
            
            // Update the setting for both AC and DC
            if (UseWmi)
            {
                return WmiHelper.UpdatePowerSetting(activePlanGuid, settingGuid, subGroupGuid, value, value);
            }
            else
            {
                return PowerProfileHelper.UpdatePowerSetting(activePlanGuid, settingGuid, subGroupGuid, value, value);
            }
        }
    }
}
