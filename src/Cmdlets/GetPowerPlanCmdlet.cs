using System;
using System.Collections.Generic;
using System.Management.Automation;
using PowerPlanTools.Models;
using PowerPlanTools.Utils;

namespace PowerPlanTools.Cmdlets
{
    /// <summary>
    /// <para type="synopsis">Gets Windows power plans.</para>
    /// <para type="description">The Get-PowerPlan cmdlet gets information about Windows power plans.</para>
    /// <para type="description">By default, it returns all power plans. You can specify a plan name or GUID to get a specific plan.</para>
    /// <example>
    ///     <para>Get all power plans</para>
    ///     <code>Get-PowerPlan</code>
    /// </example>
    /// <example>
    ///     <para>Get a specific power plan by name</para>
    ///     <code>Get-PowerPlan -Name "Balanced"</code>
    /// </example>
    /// <example>
    ///     <para>Get a specific power plan by GUID</para>
    ///     <code>Get-PowerPlan -Guid "381b4222-f694-41f0-9685-ff5bb260df2e"</code>
    /// </example>
    /// <example>
    ///     <para>Get the active power plan</para>
    ///     <code>Get-PowerPlan -Active</code>
    /// </example>
    /// </summary>
    [Cmdlet(VerbsCommon.Get, "PowerPlan")]
    [OutputType(typeof(PowerPlan))]
    public class GetPowerPlanCmdlet : PSCmdlet
    {
        /// <summary>
        /// <para type="description">Gets or sets the name of the power plan to retrieve.</para>
        /// </summary>
        [Parameter(Position = 0, ValueFromPipeline = true, ValueFromPipelineByPropertyName = true)]
        [ValidateNotNullOrEmpty]
        [ArgumentCompleter(typeof(ArgumentCompleters.PowerPlanNameCompleter))]
        public string Name { get; set; }

        /// <summary>
        /// <para type="description">Gets or sets the GUID of the power plan to retrieve.</para>
        /// </summary>
        [Parameter(ValueFromPipelineByPropertyName = true)]
        [ArgumentCompleter(typeof(ArgumentCompleters.PowerPlanGuidCompleter))]
        public Guid? Guid { get; set; }

        /// <summary>
        /// <para type="description">Gets or sets whether to retrieve only the active power plan.</para>
        /// </summary>
        [Parameter]
        public SwitchParameter Active { get; set; }

        /// <summary>
        /// <para type="description">Gets or sets whether to include power settings in the output.</para>
        /// </summary>
        [Parameter]
        public SwitchParameter IncludeSettings { get; set; }

        /// <summary>
        /// <para type="description">Gets or sets whether to include hidden power settings in the output.</para>
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
                List<PowerPlan> powerPlans;

                // Get power plans using either PowrProf.dll (default) or WMI
                if (UseWmi)
                {
                    powerPlans = WmiHelper.GetPowerPlans();
                }
                else
                {
                    powerPlans = PowerProfileHelper.GetPowerPlans();
                }

                // Filter by name, GUID, or active status
                List<PowerPlan> filteredPlans = new List<PowerPlan>();

                foreach (PowerPlan plan in powerPlans)
                {
                    bool nameMatch = string.IsNullOrEmpty(Name) || plan.Name.Equals(Name, StringComparison.OrdinalIgnoreCase);
                    bool guidMatch = !Guid.HasValue || plan.Guid == Guid.Value;
                    bool activeMatch = !Active.IsPresent || plan.IsActive;

                    if (nameMatch && guidMatch && activeMatch)
                    {
                        // Include settings if requested
                        if (IncludeSettings)
                        {
                            if (UseWmi)
                            {
                                plan.Settings = WmiHelper.GetPowerSettings(plan.Guid, IncludeHidden);
                            }
                            else
                            {
                                plan.Settings = PowerProfileHelper.GetPowerSettings(plan.Guid, IncludeHidden);
                            }
                        }

                        filteredPlans.Add(plan);
                    }
                }

                // Write output
                foreach (PowerPlan plan in filteredPlans)
                {
                    WriteObject(plan);
                }
            }
            catch (Exception ex)
            {
                WriteError(new ErrorRecord(ex, "GetPowerPlanError", ErrorCategory.NotSpecified, null));
            }
        }
    }
}
