using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using PowerPlanTools.Models;

namespace PowerPlanTools.Utils
{
    /// <summary>
    /// Helper class for event log operations related to power plans
    /// </summary>
    internal static class EventLogHelper
    {
        /// <summary>
        /// Event log name for power-related events
        /// </summary>
        private const string PowerEventLogName = "System";

        /// <summary>
        /// Event source for power-related events
        /// </summary>
        private const string PowerEventSource = "Microsoft-Windows-Kernel-Power";

        /// <summary>
        /// Event ID for power plan change events
        /// </summary>
        private const int PowerPlanChangeEventId = 105;

        /// <summary>
        /// Gets power plan statistics from event logs
        /// </summary>
        /// <param name="startTime">The start time for the statistics</param>
        /// <param name="endTime">The end time for the statistics</param>
        /// <returns>A list of PowerPlanStats objects</returns>
        public static List<PowerPlanStats> GetPowerPlanStats(DateTime startTime, DateTime endTime)
        {
            Dictionary<Guid, PowerPlanStatsBuilder> statsBuilders = new Dictionary<Guid, PowerPlanStatsBuilder>();
            
            try
            {
                EventLog systemLog = new EventLog(PowerEventLogName);
                
                // Filter entries by source and event ID
                var entries = systemLog.Entries.Cast<EventLogEntry>()
                    .Where(e => e.TimeGenerated >= startTime && e.TimeGenerated <= endTime &&
                                e.Source == PowerEventSource && e.EventID == PowerPlanChangeEventId)
                    .OrderBy(e => e.TimeGenerated)
                    .ToList();

                // Process entries to build statistics
                DateTime? lastEventTime = null;
                Guid? lastActivePlanGuid = null;

                foreach (EventLogEntry entry in entries)
                {
                    // Extract power plan GUID from event data
                    Guid planGuid = ExtractPowerPlanGuidFromEvent(entry);
                    if (planGuid == Guid.Empty)
                        continue;

                    // Get plan name from WMI
                    string planName = GetPowerPlanName(planGuid);
                    
                    // If this is not the first event, calculate duration for the previous plan
                    if (lastEventTime.HasValue && lastActivePlanGuid.HasValue)
                    {
                        TimeSpan duration = entry.TimeGenerated - lastEventTime.Value;
                        
                        if (!statsBuilders.TryGetValue(lastActivePlanGuid.Value, out PowerPlanStatsBuilder builder))
                        {
                            builder = new PowerPlanStatsBuilder
                            {
                                PlanGuid = lastActivePlanGuid.Value,
                                PlanName = GetPowerPlanName(lastActivePlanGuid.Value),
                                FirstSeen = lastEventTime.Value
                            };
                            statsBuilders[lastActivePlanGuid.Value] = builder;
                        }

                        builder.TotalDuration += duration;
                        builder.LastSeen = entry.TimeGenerated;
                        builder.ActivationCount++;
                    }

                    lastEventTime = entry.TimeGenerated;
                    lastActivePlanGuid = planGuid;
                }

                // Handle the last active plan
                if (lastEventTime.HasValue && lastActivePlanGuid.HasValue)
                {
                    TimeSpan duration = endTime - lastEventTime.Value;
                    
                    if (!statsBuilders.TryGetValue(lastActivePlanGuid.Value, out PowerPlanStatsBuilder builder))
                    {
                        builder = new PowerPlanStatsBuilder
                        {
                            PlanGuid = lastActivePlanGuid.Value,
                            PlanName = GetPowerPlanName(lastActivePlanGuid.Value),
                            FirstSeen = lastEventTime.Value
                        };
                        statsBuilders[lastActivePlanGuid.Value] = builder;
                    }

                    builder.TotalDuration += duration;
                    builder.LastSeen = endTime;
                }

                // Calculate percentages
                TimeSpan totalTime = endTime - startTime;
                double totalSeconds = totalTime.TotalSeconds;

                foreach (var builder in statsBuilders.Values)
                {
                    builder.PercentOfTimeActive = totalSeconds > 0 ? 
                        builder.TotalDuration.TotalSeconds / totalSeconds * 100 : 0;
                }

                // Build final statistics
                return statsBuilders.Values.Select(b => b.Build()).ToList();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error getting power plan stats: {ex.Message}");
                return new List<PowerPlanStats>();
            }
        }

        /// <summary>
        /// Extracts a power plan GUID from an event log entry
        /// </summary>
        /// <param name="entry">The event log entry</param>
        /// <returns>The extracted GUID</returns>
        private static Guid ExtractPowerPlanGuidFromEvent(EventLogEntry entry)
        {
            try
            {
                // This is a simplified implementation
                // In a real implementation, you would parse the event data to extract the GUID
                // The actual format depends on the event structure
                
                // For demonstration purposes, we'll just return an empty GUID
                return Guid.Empty;
            }
            catch
            {
                return Guid.Empty;
            }
        }

        /// <summary>
        /// Gets the name of a power plan from its GUID
        /// </summary>
        /// <param name="planGuid">The GUID of the power plan</param>
        /// <returns>The name of the power plan</returns>
        private static string GetPowerPlanName(Guid planGuid)
        {
            // Get the power plan name from WMI
            var plans = WmiHelper.GetPowerPlans();
            var plan = plans.FirstOrDefault(p => p.Guid == planGuid);
            return plan?.Name ?? $"Unknown Plan ({planGuid})";
        }

        /// <summary>
        /// Helper class for building PowerPlanStats objects
        /// </summary>
        private class PowerPlanStatsBuilder
        {
            public string PlanName { get; set; }
            public Guid PlanGuid { get; set; }
            public TimeSpan TotalDuration { get; set; }
            public DateTime FirstSeen { get; set; }
            public DateTime LastSeen { get; set; }
            public double PercentOfTimeActive { get; set; }
            public int ActivationCount { get; set; }

            public PowerPlanStatsBuilder()
            {
                TotalDuration = TimeSpan.Zero;
                ActivationCount = 0;
            }

            public PowerPlanStats Build()
            {
                return new PowerPlanStats(
                    PlanName,
                    PlanGuid,
                    TotalDuration,
                    FirstSeen,
                    LastSeen,
                    PercentOfTimeActive,
                    ActivationCount);
            }
        }
    }
}
