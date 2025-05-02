using System;
using System.Management.Automation;
using PowerPlanTools.Models;
using PowerPlanTools.Utils;

namespace PowerPlanTools.Cmdlets
{
    /// <summary>
    /// <para type="synopsis">Creates a new Windows power plan by cloning an existing one.</para>
    /// <para type="description">The New-PowerPlan cmdlet creates a new Windows power plan by cloning an existing one.</para>
    /// <para type="description">You can specify the source plan by name or GUID.</para>
    /// <example>
    ///     <para>Create a new power plan by cloning an existing one</para>
    ///     <code>New-PowerPlan -SourcePlanName "Balanced" -NewPlanName "My Custom Plan"</code>
    /// </example>
    /// <example>
    ///     <para>Create a new power plan by cloning an existing one using GUID</para>
    ///     <code>New-PowerPlan -SourcePlanGuid "381b4222-f694-41f0-9685-ff5bb260df2e" -NewPlanName "My Custom Plan"</code>
    /// </example>
    /// <example>
    ///     <para>Create a new power plan and return the plan object</para>
    ///     <code>New-PowerPlan -SourcePlanName "Balanced" -NewPlanName "My Custom Plan" -PassThru</code>
    /// </example>
    /// </summary>
    [Cmdlet(VerbsCommon.New, "PowerPlan", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
    [OutputType(typeof(PowerPlan))]
    public class NewPowerPlanCmdlet : PSCmdlet
    {
        /// <summary>
        /// <para type="description">Gets or sets the name of the source power plan to clone.</para>
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipeline = true, ValueFromPipelineByPropertyName = true, ParameterSetName = "ByName")]
        [ValidateNotNullOrEmpty]
        public string SourcePlanName { get; set; }

        /// <summary>
        /// <para type="description">Gets or sets the GUID of the source power plan to clone.</para>
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = true, ParameterSetName = "ByGuid")]
        public Guid SourcePlanGuid { get; set; }

        /// <summary>
        /// <para type="description">Gets or sets the name for the new power plan.</para>
        /// </summary>
        [Parameter(Mandatory = true, Position = 0)]
        [ValidateNotNullOrEmpty]
        public string NewPlanName { get; set; }

        /// <summary>
        /// <para type="description">Gets or sets whether to return the new power plan.</para>
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
                // Get the source power plan GUID
                Guid sourcePlanGuid;
                string sourcePlanName;

                if (ParameterSetName == "ByName")
                {
                    // Find the plan by name
                    var powerPlans = UseWmi ? WmiHelper.GetPowerPlans() : PowerProfileHelper.GetPowerPlans();
                    var plan = powerPlans.Find(p => p.Name.Equals(SourcePlanName, StringComparison.OrdinalIgnoreCase));

                    if (plan == null)
                    {
                        WriteError(new ErrorRecord(
                            new ArgumentException($"Source power plan '{SourcePlanName}' not found."),
                            "SourcePowerPlanNotFound",
                            ErrorCategory.ObjectNotFound,
                            SourcePlanName));
                        return;
                    }

                    sourcePlanGuid = plan.Guid;
                    sourcePlanName = plan.Name;
                }
                else
                {
                    // Use the provided GUID
                    sourcePlanGuid = SourcePlanGuid;

                    // Get the plan name for display
                    var powerPlans = UseWmi ? WmiHelper.GetPowerPlans() : PowerProfileHelper.GetPowerPlans();
                    var plan = powerPlans.Find(p => p.Guid == sourcePlanGuid);
                    sourcePlanName = plan?.Name ?? sourcePlanGuid.ToString();
                }

                // Confirm the action
                if (!ShouldProcess($"Create new power plan '{NewPlanName}' from '{sourcePlanName}'", "New-PowerPlan"))
                {
                    return;
                }

                // Create the new power plan
                Guid newPlanGuid;
                if (UseWmi)
                {
                    newPlanGuid = WmiHelper.CreatePowerPlan(sourcePlanGuid, NewPlanName);
                }
                else
                {
                    newPlanGuid = PowerProfileHelper.CreatePowerPlan(sourcePlanGuid, NewPlanName);
                }

                if (newPlanGuid == Guid.Empty)
                {
                    WriteError(new ErrorRecord(
                        new InvalidOperationException($"Failed to create new power plan '{NewPlanName}' from '{sourcePlanName}'."),
                        "CreatePowerPlanFailed",
                        ErrorCategory.InvalidOperation,
                        NewPlanName));
                    return;
                }

                // Return the new power plan if requested
                if (PassThru)
                {
                    var powerPlans = UseWmi ? WmiHelper.GetPowerPlans() : PowerProfileHelper.GetPowerPlans();
                    var newPlan = powerPlans.Find(p => p.Guid == newPlanGuid);
                    if (newPlan != null)
                    {
                        WriteObject(newPlan);
                    }
                }
            }
            catch (Exception ex)
            {
                WriteError(new ErrorRecord(ex, "NewPowerPlanError", ErrorCategory.NotSpecified, null));
            }
        }
    }
}
