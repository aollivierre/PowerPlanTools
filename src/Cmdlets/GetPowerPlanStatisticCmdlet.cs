using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using PowerPlanTools.Models;
using PowerPlanTools.Utils;

namespace PowerPlanTools.Cmdlets
{
    /// <summary>
    /// <para type="synopsis">Generates power plan usage statistics.</para>
    /// <para type="description">The Get-PowerPlanStatistic cmdlet generates usage statistics for Windows power plans.</para>
    /// <para type="description">It uses the event log to track when power plans were active.</para>
    /// <example>
    ///     <para>Get power plan statistics for the last 7 days</para>
    ///     <code>Get-PowerPlanStatistic -Days 7</code>
    /// </example>
    /// <example>
    ///     <para>Get power plan statistics for a specific time period</para>
    ///     <code>Get-PowerPlanStatistic -StartTime (Get-Date).AddDays(-30) -EndTime (Get-Date)</code>
    /// </example>
    /// <example>
    ///     <para>Get statistics for a specific power plan</para>
    ///     <code>Get-PowerPlanStatistic -Days 7 -PlanName "Balanced"</code>
    /// </example>
    /// </summary>
    [Cmdlet(VerbsCommon.Get, "PowerPlanStatistic")]
    [OutputType(typeof(PowerPlanStats))]
    public class GetPowerPlanStatisticCmdlet : PSCmdlet
    {
        /// <summary>
        /// <para type="description">Gets or sets the number of days to include in the report.</para>
        /// </summary>
        [Parameter(ParameterSetName = "ByDays")]
        [ValidateRange(1, 365)]
        public int Days { get; set; } = 7;

        /// <summary>
        /// <para type="description">Gets or sets the start time for the report.</para>
        /// </summary>
        [Parameter(Mandatory = true, ParameterSetName = "ByTimeRange")]
        public DateTime StartTime { get; set; }

        /// <summary>
        /// <para type="description">Gets or sets the end time for the report.</para>
        /// </summary>
        [Parameter(Mandatory = true, ParameterSetName = "ByTimeRange")]
        public DateTime EndTime { get; set; }

        /// <summary>
        /// <para type="description">Gets or sets the name of a specific power plan to report on.</para>
        /// </summary>
        [Parameter(ValueFromPipeline = true, ValueFromPipelineByPropertyName = true)]
        [ValidateNotNullOrEmpty]
        public string PlanName { get; set; }

        /// <summary>
        /// <para type="description">Gets or sets the GUID of a specific power plan to report on.</para>
        /// </summary>
        [Parameter(ValueFromPipelineByPropertyName = true)]
        public Guid? PlanGuid { get; set; }

        /// <summary>
        /// Processes the cmdlet.
        /// </summary>
        protected override void ProcessRecord()
        {
            try
            {
                // Determine the time range
                DateTime startTime;
                DateTime endTime;

                if (ParameterSetName == "ByDays")
                {
                    endTime = DateTime.Now;
                    startTime = endTime.AddDays(-Days);
                }
                else
                {
                    startTime = StartTime;
                    endTime = EndTime;
                }

                // Validate time range
                if (startTime >= endTime)
                {
                    WriteError(new ErrorRecord(
                        new ArgumentException("Start time must be earlier than end time."),
                        "InvalidTimeRange",
                        ErrorCategory.InvalidArgument,
                        null));
                    return;
                }

                // Get power plan statistics
                List<PowerPlanStats> stats = EventLogHelper.GetPowerPlanStats(startTime, endTime);

                // Filter by plan name or GUID if specified
                if (!string.IsNullOrEmpty(PlanName))
                {
                    stats = stats.Where(s => s.PlanName.Equals(PlanName, StringComparison.OrdinalIgnoreCase)).ToList();
                }

                if (PlanGuid.HasValue)
                {
                    stats = stats.Where(s => s.PlanGuid == PlanGuid.Value).ToList();
                }

                // Write output
                foreach (PowerPlanStats stat in stats)
                {
                    WriteObject(stat);
                }

                // Write summary
                LoggingHelper.LogVerbose(this, $"Generated power plan statistics from {startTime} to {endTime}.");
                LoggingHelper.LogVerbose(this, $"Found {stats.Count} power plans with activity during this period.");
            }
            catch (Exception ex)
            {
                WriteError(new ErrorRecord(ex, "GetPowerPlanStatisticError", ErrorCategory.NotSpecified, null));
            }
        }
    }
}
