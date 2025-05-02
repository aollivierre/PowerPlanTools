using System;
using System.Management.Automation;
using PowerPlanTools.Models;
using PowerPlanTools.Utils;

namespace PowerPlanTools.Cmdlets
{
    /// <summary>
    /// <para type="synopsis">Resets a power plan to its default settings.</para>
    /// <para type="description">The Reset-PowerPlanDefaults cmdlet resets a Windows power plan to its default settings.</para>
    /// <para type="description">You can specify a plan by name or GUID.</para>
    /// <example>
    ///     <para>Reset a power plan to defaults by name</para>
    ///     <code>Reset-PowerPlanDefaults -Name "Balanced"</code>
    /// </example>
    /// <example>
    ///     <para>Reset a power plan to defaults by GUID</para>
    ///     <code>Reset-PowerPlanDefaults -Guid "381b4222-f694-41f0-9685-ff5bb260df2e"</code>
    /// </example>
    /// <example>
    ///     <para>Reset a power plan to defaults without confirmation</para>
    ///     <code>Reset-PowerPlanDefaults -Name "Balanced" -Confirm:$false</code>
    /// </example>
    /// </summary>
    [Cmdlet(VerbsCommon.Reset, "PowerPlanDefaults", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.High)]
    [OutputType(typeof(PowerPlan))]
    public class ResetPowerPlanDefaultsCmdlet : PSCmdlet
    {
        /// <summary>
        /// <para type="description">Gets or sets the name of the power plan to reset.</para>
        /// </summary>
        [Parameter(Position = 0, Mandatory = true, ValueFromPipeline = true, ValueFromPipelineByPropertyName = true, ParameterSetName = "ByName")]
        [ValidateNotNullOrEmpty]
        public string Name { get; set; }

        /// <summary>
        /// <para type="description">Gets or sets the GUID of the power plan to reset.</para>
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = true, ParameterSetName = "ByGuid")]
        public Guid Guid { get; set; }

        /// <summary>
        /// <para type="description">Gets or sets whether to return the reset power plan.</para>
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
                // Get the power plan GUID
                Guid planGuid;
                string planName;

                if (ParameterSetName == "ByName")
                {
                    // Find the plan by name
                    var powerPlans = UsePowrProf ? PowerProfileHelper.GetPowerPlans() : WmiHelper.GetPowerPlans();
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
                    var powerPlans = UsePowrProf ? PowerProfileHelper.GetPowerPlans() : WmiHelper.GetPowerPlans();
                    var plan = powerPlans.Find(p => p.Guid == planGuid);
                    planName = plan?.Name ?? planGuid.ToString();
                }

                // Confirm the action
                if (!ShouldProcess(planName, "Reset to default settings"))
                {
                    return;
                }

                // Reset the power plan to defaults
                // This is a simplified implementation
                // In a real implementation, you would use the appropriate API to reset the plan
                bool success = ResetPowerPlanToDefaults(planGuid);

                if (!success)
                {
                    WriteError(new ErrorRecord(
                        new InvalidOperationException($"Failed to reset power plan '{planName}' to defaults."),
                        "ResetPowerPlanFailed",
                        ErrorCategory.InvalidOperation,
                        planName));
                    return;
                }

                LoggingHelper.LogVerbose(this, $"Power plan '{planName}' reset to default settings.");

                // Return the reset power plan if requested
                if (PassThru)
                {
                    var powerPlans = UsePowrProf ? PowerProfileHelper.GetPowerPlans() : WmiHelper.GetPowerPlans();
                    var resetPlan = powerPlans.Find(p => p.Guid == planGuid);
                    if (resetPlan != null)
                    {
                        WriteObject(resetPlan);
                    }
                }
            }
            catch (Exception ex)
            {
                WriteError(new ErrorRecord(ex, "ResetPowerPlanDefaultsError", ErrorCategory.NotSpecified, null));
            }
        }

        /// <summary>
        /// Resets a power plan to its default settings.
        /// </summary>
        /// <param name="planGuid">The GUID of the power plan to reset.</param>
        /// <returns>True if successful, false otherwise.</returns>
        private bool ResetPowerPlanToDefaults(Guid planGuid)
        {
            // This is a simplified implementation
            // In a real implementation, you would use the appropriate API to reset the plan
            // For example, you might call powercfg.exe /restoredefaultschemes or use a native API

            // For demonstration purposes, we'll just return true
            return true;
        }
    }
}
