using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using PowerPlanTools.Models;
using PowerPlanTools.Utils;

namespace PowerPlanTools.Cmdlets
{
    /// <summary>
    /// <para type="synopsis">Gets power settings for a Windows power plan.</para>
    /// <para type="description">The Get-PowerSetting cmdlet gets power settings for a Windows power plan.</para>
    /// <para type="description">You can specify a plan by name or GUID, and filter settings by alias or GUID.</para>
    /// <example>
    ///     <para>Get all power settings for a plan</para>
    ///     <code>Get-PowerSetting -PlanName "Balanced"</code>
    /// </example>
    /// <example>
    ///     <para>Get a specific power setting by alias</para>
    ///     <code>Get-PowerSetting -PlanName "Balanced" -SettingAlias "Turn off display after"</code>
    /// </example>
    /// <example>
    ///     <para>Get a specific power setting by GUID</para>
    ///     <code>Get-PowerSetting -PlanGuid "381b4222-f694-41f0-9685-ff5bb260df2e" -SettingGuid "7516b95f-f776-4464-8c53-06167f40cc99"</code>
    /// </example>
    /// <example>
    ///     <para>Get all power settings including hidden ones</para>
    ///     <code>Get-PowerSetting -PlanName "Balanced" -IncludeHidden</code>
    /// </example>
    /// </summary>
    [Cmdlet(VerbsCommon.Get, "PowerSetting")]
    [OutputType(typeof(PowerSetting))]
    public class GetPowerSettingCmdlet : PSCmdlet
    {
        /// <summary>
        /// <para type="description">Gets or sets the name of the power plan.</para>
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipeline = true, ValueFromPipelineByPropertyName = true, ParameterSetName = "ByPlanName")]
        [ValidateNotNullOrEmpty]
        [ArgumentCompleter(typeof(ArgumentCompleters.PowerPlanNameCompleter))]
        public string PlanName { get; set; }

        /// <summary>
        /// <para type="description">Gets or sets the GUID of the power plan.</para>
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = true, ParameterSetName = "ByPlanGuid")]
        [ArgumentCompleter(typeof(ArgumentCompleters.PowerPlanGuidCompleter))]
        public Guid PlanGuid { get; set; }

        /// <summary>
        /// <para type="description">Gets or sets the alias of the setting to retrieve.</para>
        /// </summary>
        [Parameter(ValueFromPipelineByPropertyName = true)]
        [ValidateNotNullOrEmpty]
        [ArgumentCompleter(typeof(ArgumentCompleters.PowerSettingAliasCompleter))]
        public string SettingAlias { get; set; }

        /// <summary>
        /// <para type="description">Gets or sets the GUID of the setting to retrieve.</para>
        /// </summary>
        [Parameter(ValueFromPipelineByPropertyName = true)]
        [ArgumentCompleter(typeof(ArgumentCompleters.PowerSettingGuidCompleter))]
        public Guid? SettingGuid { get; set; }

        /// <summary>
        /// <para type="description">Gets or sets the GUID of the subgroup containing the setting.</para>
        /// </summary>
        [Parameter(ValueFromPipelineByPropertyName = true)]
        [ArgumentCompleter(typeof(ArgumentCompleters.SubGroupGuidCompleter))]
        public Guid? SubGroupGuid { get; set; }

        /// <summary>
        /// <para type="description">Gets or sets whether to include hidden settings in the output.</para>
        /// </summary>
        [Parameter]
        public SwitchParameter IncludeHidden { get; set; }

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
                // Get the power plan GUID
                Guid planGuid;

                if (ParameterSetName == "ByPlanName")
                {
                    // Find the plan by name
                    var powerPlans = UseWmi ? WmiHelper.GetPowerPlans() : PowerProfileHelper.GetPowerPlans();
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
                }
                else
                {
                    // Use the provided GUID
                    planGuid = PlanGuid;
                }

                // Get power settings
                List<PowerSetting> settings;
                if (UseWmi)
                {
                    settings = WmiHelper.GetPowerSettings(planGuid, IncludeHidden);
                }
                else
                {
                    settings = PowerProfileHelper.GetPowerSettings(planGuid, IncludeHidden);
                }

                // Filter settings
                IEnumerable<PowerSetting> filteredSettings = settings;

                if (!string.IsNullOrEmpty(SettingAlias))
                {
                    filteredSettings = filteredSettings.Where(s => s.Alias.Equals(SettingAlias, StringComparison.OrdinalIgnoreCase));
                }

                if (SettingGuid.HasValue)
                {
                    filteredSettings = filteredSettings.Where(s => s.SettingGuid == SettingGuid.Value);
                }

                if (SubGroupGuid.HasValue)
                {
                    filteredSettings = filteredSettings.Where(s => s.SubGroupGuid == SubGroupGuid.Value);
                }

                // Write output
                foreach (PowerSetting setting in filteredSettings)
                {
                    WriteObject(setting);
                }
            }
            catch (Exception ex)
            {
                WriteError(new ErrorRecord(ex, "GetPowerSettingError", ErrorCategory.NotSpecified, null));
            }
        }
    }
}
