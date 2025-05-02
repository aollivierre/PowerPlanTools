using System;
using System.Management.Automation;
using PowerPlanTools.Utils;

namespace PowerPlanTools.Cmdlets
{
    /// <summary>
    /// <para type="synopsis">Deletes a Windows power plan.</para>
    /// <para type="description">The Remove-PowerPlan cmdlet deletes a Windows power plan.</para>
    /// <para type="description">You can specify a plan by name or GUID.</para>
    /// <example>
    ///     <para>Delete a power plan by name</para>
    ///     <code>Remove-PowerPlan -Name "My Custom Plan"</code>
    /// </example>
    /// <example>
    ///     <para>Delete a power plan by GUID</para>
    ///     <code>Remove-PowerPlan -Guid "381b4222-f694-41f0-9685-ff5bb260df2e"</code>
    /// </example>
    /// <example>
    ///     <para>Delete a power plan without confirmation</para>
    ///     <code>Remove-PowerPlan -Name "My Custom Plan" -Force</code>
    /// </example>
    /// </summary>
    [Cmdlet(VerbsCommon.Remove, "PowerPlan", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.High)]
    public class RemovePowerPlanCmdlet : PSCmdlet
    {
        /// <summary>
        /// <para type="description">Gets or sets the name of the power plan to delete.</para>
        /// </summary>
        [Parameter(Position = 0, Mandatory = true, ValueFromPipeline = true, ValueFromPipelineByPropertyName = true, ParameterSetName = "ByName")]
        [ValidateNotNullOrEmpty]
        public string Name { get; set; }

        /// <summary>
        /// <para type="description">Gets or sets the GUID of the power plan to delete.</para>
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = true, ParameterSetName = "ByGuid")]
        public Guid Guid { get; set; }

        /// <summary>
        /// <para type="description">Gets or sets whether to suppress confirmation prompts.</para>
        /// </summary>
        [Parameter]
        public SwitchParameter Force { get; set; }

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

                // Check if the plan is active
                var activePlanGuid = UseWmi ? WmiHelper.GetActivePowerPlanGuid() : PowerProfileHelper.GetActivePowerPlanGuid();
                if (planGuid == activePlanGuid)
                {
                    WriteError(new ErrorRecord(
                        new InvalidOperationException($"Cannot delete the active power plan '{planName}'. Set another plan as active first."),
                        "CannotDeleteActivePlan",
                        ErrorCategory.InvalidOperation,
                        planName));
                    return;
                }

                // Confirm the action
                if (!Force && !ShouldProcess(planName, "Delete power plan"))
                {
                    return;
                }

                // Delete the power plan
                bool success;
                if (UseWmi)
                {
                    success = WmiHelper.DeletePowerPlan(planGuid);
                }
                else
                {
                    success = PowerProfileHelper.DeletePowerPlan(planGuid);
                }

                if (!success)
                {
                    WriteError(new ErrorRecord(
                        new InvalidOperationException($"Failed to delete power plan '{planName}'."),
                        "DeletePowerPlanFailed",
                        ErrorCategory.InvalidOperation,
                        planName));
                    return;
                }

                LoggingHelper.LogVerbose(this, $"Power plan '{planName}' deleted successfully.");
            }
            catch (Exception ex)
            {
                WriteError(new ErrorRecord(ex, "RemovePowerPlanError", ErrorCategory.NotSpecified, null));
            }
        }
    }
}
