using System;
using System.Management.Automation;
using PowerPlanTools.Models;
using PowerPlanTools.Utils;

namespace PowerPlanTools.Cmdlets
{
    /// <summary>
    /// <para type="synopsis">Updates a power setting value.</para>
    /// <para type="description">The Update-PowerSetting cmdlet updates a power setting value for a Windows power plan.</para>
    /// <para type="description">You can specify a plan and setting by name or GUID.</para>
    /// <example>
    ///     <para>Update a power setting by alias</para>
    ///     <code>Update-PowerSetting -PlanName "Balanced" -SettingAlias "Turn off display after" -PluggedIn 15 -OnBattery 5</code>
    /// </example>
    /// <example>
    ///     <para>Update a power setting by GUID</para>
    ///     <code>Update-PowerSetting -PlanGuid "381b4222-f694-41f0-9685-ff5bb260df2e" -SettingGuid "7516b95f-f776-4464-8c53-06167f40cc99" -PluggedIn 15</code>
    /// </example>
    /// <example>
    ///     <para>Update a power setting and return the updated setting</para>
    ///     <code>Update-PowerSetting -PlanName "Balanced" -SettingAlias "Turn off display after" -PluggedIn 15 -PassThru</code>
    /// </example>
    /// </summary>
    [Cmdlet(VerbsData.Update, "PowerSetting", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
    [OutputType(typeof(PowerSetting))]
    public class UpdatePowerSettingCmdlet : PSCmdlet
    {
        /// <summary>
        /// <para type="description">Gets or sets the name of the power plan.</para>
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipeline = true, ValueFromPipelineByPropertyName = true, ParameterSetName = "ByPlanNameSettingAlias")]
        [Parameter(Mandatory = true, ValueFromPipeline = true, ValueFromPipelineByPropertyName = true, ParameterSetName = "ByPlanNameSettingGuid")]
        [ValidateNotNullOrEmpty]
        [ArgumentCompleter(typeof(ArgumentCompleters.PowerPlanNameCompleter))]
        public string PlanName { get; set; }

        /// <summary>
        /// <para type="description">Gets or sets the GUID of the power plan.</para>
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = true, ParameterSetName = "ByPlanGuidSettingAlias")]
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = true, ParameterSetName = "ByPlanGuidSettingGuid")]
        [ArgumentCompleter(typeof(ArgumentCompleters.PowerPlanGuidCompleter))]
        public Guid PlanGuid { get; set; }

        /// <summary>
        /// <para type="description">Gets or sets the alias of the setting to update.</para>
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = true, ParameterSetName = "ByPlanNameSettingAlias")]
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = true, ParameterSetName = "ByPlanGuidSettingAlias")]
        [ValidateNotNullOrEmpty]
        [ArgumentCompleter(typeof(ArgumentCompleters.PowerSettingAliasCompleter))]
        public string SettingAlias { get; set; }

        /// <summary>
        /// <para type="description">Gets or sets the GUID of the setting to update.</para>
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = true, ParameterSetName = "ByPlanNameSettingGuid")]
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = true, ParameterSetName = "ByPlanGuidSettingGuid")]
        [ArgumentCompleter(typeof(ArgumentCompleters.PowerSettingGuidCompleter))]
        public Guid SettingGuid { get; set; }

        /// <summary>
        /// <para type="description">Gets or sets the GUID of the subgroup containing the setting.</para>
        /// </summary>
        [Parameter(ValueFromPipelineByPropertyName = true, ParameterSetName = "ByPlanNameSettingGuid")]
        [Parameter(ValueFromPipelineByPropertyName = true, ParameterSetName = "ByPlanGuidSettingGuid")]
        [ArgumentCompleter(typeof(ArgumentCompleters.SubGroupGuidCompleter))]
        public Guid? SubGroupGuid { get; set; }

        /// <summary>
        /// <para type="description">Gets or sets the value to set when plugged in.</para>
        /// </summary>
        [Parameter]
        public object PluggedIn { get; set; }

        /// <summary>
        /// <para type="description">Gets or sets the value to set when on battery.</para>
        /// </summary>
        [Parameter]
        public object OnBattery { get; set; }

        /// <summary>
        /// <para type="description">Gets or sets whether to return the updated setting.</para>
        /// </summary>
        [Parameter]
        public SwitchParameter PassThru { get; set; }

        /// <summary>
        /// <para type="description">Gets or sets whether to use PowrProf.dll instead of WMI.</para>
        /// </summary>
        [Parameter]
        public SwitchParameter UsePowrProf { get; set; }

        /// <summary>
        /// Processes the cmdlet.
        /// </summary>
        protected override void ProcessRecord()
        {
            try
            {
                // Validate that at least one value is provided
                if (PluggedIn == null && OnBattery == null)
                {
                    WriteError(new ErrorRecord(
                        new ArgumentException("At least one of -PluggedIn or -OnBattery must be specified."),
                        "NoValueSpecified",
                        ErrorCategory.InvalidArgument,
                        null));
                    return;
                }

                // Get the power plan GUID
                Guid planGuid;
                string planName;

                if (ParameterSetName.StartsWith("ByPlanName"))
                {
                    // Find the plan by name
                    var powerPlans = UsePowrProf ? PowerProfileHelper.GetPowerPlans() : WmiHelper.GetPowerPlans();
                    var plan = powerPlans.Find(p => p.Name.Equals(PlanName, StringComparison.OrdinalIgnoreCase));

                    if (plan == null)
                    {
                        WriteError(new ErrorRecord(
                            new ArgumentException($"Power plan '{PlanName}' not found."),
                            "PowerPlanNotFound",
                            ErrorCategory.ObjectNotFound,
                            PlanName));
                        return;
                    }

                    planGuid = plan.Guid;
                    planName = plan.Name;
                }
                else
                {
                    // Use the provided GUID
                    planGuid = PlanGuid;

                    // Get the plan name for display
                    var powerPlans = UsePowrProf ? PowerProfileHelper.GetPowerPlans() : WmiHelper.GetPowerPlans();
                    var plan = powerPlans.Find(p => p.Guid == planGuid);
                    planName = plan?.Name ?? planGuid.ToString();
                }

                // Get the setting GUID and subgroup GUID
                Guid settingGuid;
                Guid subGroupGuid;
                string settingName;

                if (ParameterSetName.EndsWith("SettingAlias"))
                {
                    // Find the setting by alias
                    var settings = UsePowrProf ?
                        PowerProfileHelper.GetPowerSettings(planGuid, true) :
                        WmiHelper.GetPowerSettings(planGuid, true);

                    var setting = settings.Find(s => s.Alias.Equals(SettingAlias, StringComparison.OrdinalIgnoreCase));

                    if (setting == null)
                    {
                        WriteError(new ErrorRecord(
                            new ArgumentException($"Setting '{SettingAlias}' not found in power plan '{planName}'."),
                            "SettingNotFound",
                            ErrorCategory.ObjectNotFound,
                            SettingAlias));
                        return;
                    }

                    settingGuid = setting.SettingGuid;
                    subGroupGuid = setting.SubGroupGuid;
                    settingName = setting.Alias;
                }
                else
                {
                    // Use the provided GUID
                    settingGuid = SettingGuid;

                    // If subgroup GUID is not provided, try to find it
                    if (!SubGroupGuid.HasValue)
                    {
                        var settings = UsePowrProf ?
                            PowerProfileHelper.GetPowerSettings(planGuid, true) :
                            WmiHelper.GetPowerSettings(planGuid, true);

                        var setting = settings.Find(s => s.SettingGuid == settingGuid);

                        if (setting == null)
                        {
                            WriteError(new ErrorRecord(
                                new ArgumentException($"Setting with GUID '{settingGuid}' not found in power plan '{planName}'."),
                                "SettingNotFound",
                                ErrorCategory.ObjectNotFound,
                                settingGuid));
                            return;
                        }

                        subGroupGuid = setting.SubGroupGuid;
                        settingName = setting.Alias;
                    }
                    else
                    {
                        subGroupGuid = SubGroupGuid.Value;
                        settingName = settingGuid.ToString();
                    }
                }

                // Convert values to appropriate types
                uint? acValue = PluggedIn != null ? Convert.ToUInt32(PluggedIn) : (uint?)null;
                uint? dcValue = OnBattery != null ? Convert.ToUInt32(OnBattery) : (uint?)null;

                // Confirm the action
                if (!ShouldProcess($"Update setting '{settingName}' in power plan '{planName}'", "Update-PowerSetting"))
                {
                    return;
                }

                // Update the setting
                bool success;
                if (UsePowrProf)
                {
                    success = PowerProfileHelper.UpdatePowerSetting(planGuid, settingGuid, subGroupGuid, acValue, dcValue);
                }
                else
                {
                    success = WmiHelper.UpdatePowerSetting(planGuid, settingGuid, subGroupGuid, acValue, dcValue);
                }

                if (!success)
                {
                    WriteError(new ErrorRecord(
                        new InvalidOperationException($"Failed to update setting '{settingName}' in power plan '{planName}'."),
                        "UpdateSettingFailed",
                        ErrorCategory.InvalidOperation,
                        settingName));
                    return;
                }

                LoggingHelper.LogVerbose(this, $"Setting '{settingName}' in power plan '{planName}' updated successfully.");

                // Return the updated setting if requested
                if (PassThru)
                {
                    var settings = UsePowrProf ?
                        PowerProfileHelper.GetPowerSettings(planGuid, true) :
                        WmiHelper.GetPowerSettings(planGuid, true);

                    var updatedSetting = settings.Find(s => s.SettingGuid == settingGuid);

                    if (updatedSetting != null)
                    {
                        WriteObject(updatedSetting);
                    }
                }
            }
            catch (Exception ex)
            {
                WriteError(new ErrorRecord(ex, "UpdatePowerSettingError", ErrorCategory.NotSpecified, null));
            }
        }
    }
}
