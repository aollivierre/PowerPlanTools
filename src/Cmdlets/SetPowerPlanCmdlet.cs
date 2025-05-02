using System;
using System.Management.Automation;
using PowerPlanTools.Models;
using PowerPlanTools.Utils;

namespace PowerPlanTools.Cmdlets
{
    /// <summary>
    /// <para type="synopsis">Sets the active Windows power plan.</para>
    /// <para type="description">The Set-PowerPlan cmdlet sets the active Windows power plan.</para>
    /// <para type="description">You can specify a plan by name or GUID.</para>
    /// <example>
    ///     <para>Set the active power plan by name</para>
    ///     <code>Set-PowerPlan -Name "Balanced"</code>
    /// </example>
    /// <example>
    ///     <para>Set the active power plan by GUID</para>
    ///     <code>Set-PowerPlan -Guid "381b4222-f694-41f0-9685-ff5bb260df2e"</code>
    /// </example>
    /// <example>
    ///     <para>Set the active power plan and return the plan object</para>
    ///     <code>Set-PowerPlan -Name "Balanced" -PassThru</code>
    /// </example>
    /// </summary>
    [Cmdlet(VerbsCommon.Set, "PowerPlan", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
    [OutputType(typeof(PowerPlan))]
    public class SetPowerPlanCmdlet : PSCmdlet
    {
        /// <summary>
        /// <para type="description">Gets or sets the name of the power plan to activate.</para>
        /// </summary>
        [Parameter(Position = 0, Mandatory = true, ValueFromPipeline = true, ValueFromPipelineByPropertyName = true, ParameterSetName = "ByName")]
        [ValidateNotNullOrEmpty]
        [ArgumentCompleter(typeof(ArgumentCompleters.PowerPlanNameCompleter))]
        public string Name { get; set; }

        /// <summary>
        /// <para type="description">Gets or sets the GUID of the power plan to activate.</para>
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = true, ParameterSetName = "ByGuid")]
        [ArgumentCompleter(typeof(ArgumentCompleters.PowerPlanGuidCompleter))]
        public Guid Guid { get; set; }

        /// <summary>
        /// <para type="description">Gets or sets whether to return the activated power plan.</para>
        /// </summary>
        [Parameter]
        public SwitchParameter PassThru { get; set; }

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
                string planName;

                if (ParameterSetName == "ByName")
                {
                    // Find the plan by name
                    var powerPlans = UseWmi ? WmiHelper.GetPowerPlans() : PowerProfileHelper.GetPowerPlans();
                    var plan = powerPlans.Find(p => p.Name.Equals(Name, StringComparison.OrdinalIgnoreCase));

                    if (plan == null)
                    {
                        WriteError(new ErrorRecord(
                            new ArgumentException($"Power plan '{Name}' not found."),
                            "PowerPlanNotFound",
                            ErrorCategory.ObjectNotFound,
                            Name));
                        return;
                    }

                    planGuid = plan.Guid;
                    planName = plan.Name;
                }
                else
                {
                    // Use the provided GUID
                    planGuid = Guid;

                    // Get the plan name for display
                    var powerPlans = UseWmi ? WmiHelper.GetPowerPlans() : PowerProfileHelper.GetPowerPlans();
                    var plan = powerPlans.Find(p => p.Guid == planGuid);
                    planName = plan?.Name ?? planGuid.ToString();
                }

                // Confirm the action
                if (!ShouldProcess(planName, "Set as active power plan"))
                {
                    return;
                }

                // Set the active power plan
                bool success;
                if (UseWmi)
                {
                    success = WmiHelper.SetActivePowerPlan(planGuid);
                }
                else
                {
                    success = PowerProfileHelper.SetActivePowerPlan(planGuid);
                }

                if (!success)
                {
                    WriteError(new ErrorRecord(
                        new InvalidOperationException($"Failed to set power plan '{planName}' as active."),
                        "SetPowerPlanFailed",
                        ErrorCategory.InvalidOperation,
                        planName));
                    return;
                }

                // Return the power plan if requested
                if (PassThru)
                {
                    var powerPlans = UseWmi ? WmiHelper.GetPowerPlans() : PowerProfileHelper.GetPowerPlans();
                    var activePlan = powerPlans.Find(p => p.Guid == planGuid);
                    if (activePlan != null)
                    {
                        WriteObject(activePlan);
                    }
                }
            }
            catch (Exception ex)
            {
                WriteError(new ErrorRecord(ex, "SetPowerPlanError", ErrorCategory.NotSpecified, null));
            }
        }
    }
}
