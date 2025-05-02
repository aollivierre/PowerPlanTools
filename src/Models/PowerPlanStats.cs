using System;

namespace PowerPlanTools.Models
{
    /// <summary>
    /// Represents statistics for a power plan
    /// </summary>
    public class PowerPlanStats
    {
        /// <summary>
        /// Gets or sets the name of the power plan
        /// </summary>
        public string PlanName { get; set; }

        /// <summary>
        /// Gets or sets the GUID of the power plan
        /// </summary>
        public Guid PlanGuid { get; set; }

        /// <summary>
        /// Gets or sets the total duration this plan has been active
        /// </summary>
        public TimeSpan TotalDuration { get; set; }

        /// <summary>
        /// Gets or sets the first time this plan was seen active
        /// </summary>
        public DateTime FirstSeen { get; set; }

        /// <summary>
        /// Gets or sets the last time this plan was seen active
        /// </summary>
        public DateTime LastSeen { get; set; }

        /// <summary>
        /// Gets or sets the percentage of time this plan has been active
        /// </summary>
        public double PercentOfTimeActive { get; set; }

        /// <summary>
        /// Gets or sets the number of times this plan has been activated
        /// </summary>
        public int ActivationCount { get; set; }

        /// <summary>
        /// Initializes a new instance of the PowerPlanStats class
        /// </summary>
        public PowerPlanStats()
        {
        }

        /// <summary>
        /// Initializes a new instance of the PowerPlanStats class with specified values
        /// </summary>
        /// <param name="planName">The name of the power plan</param>
        /// <param name="planGuid">The GUID of the power plan</param>
        /// <param name="totalDuration">The total duration this plan has been active</param>
        /// <param name="firstSeen">The first time this plan was seen active</param>
        /// <param name="lastSeen">The last time this plan was seen active</param>
        /// <param name="percentOfTimeActive">The percentage of time this plan has been active</param>
        /// <param name="activationCount">The number of times this plan has been activated</param>
        public PowerPlanStats(string planName, Guid planGuid, TimeSpan totalDuration, DateTime firstSeen, DateTime lastSeen, double percentOfTimeActive, int activationCount)
        {
            PlanName = planName;
            PlanGuid = planGuid;
            TotalDuration = totalDuration;
            FirstSeen = firstSeen;
            LastSeen = lastSeen;
            PercentOfTimeActive = percentOfTimeActive;
            ActivationCount = activationCount;
        }

        /// <summary>
        /// Returns a string that represents the current object
        /// </summary>
        /// <returns>A string that represents the current object</returns>
        public override string ToString()
        {
            return $"{PlanName} ({PlanGuid}) - {PercentOfTimeActive:P2} active";
        }
    }
}
