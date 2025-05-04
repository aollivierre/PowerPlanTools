using System;

namespace PowerPlanTools.Models
{
    /// <summary>
    /// Represents a possible value for a power setting.
    /// </summary>
    public class PowerSettingPossibleValue
    {
        /// <summary>
        /// Gets or sets the friendly name of the value.
        /// </summary>
        public string FriendlyName { get; set; }

        /// <summary>
        /// Gets or sets the actual numeric value.
        /// </summary>
        public uint ActualValue { get; set; }

        /// <summary>
        /// Returns a string representation of the possible value.
        /// </summary>
        public override string ToString()
        {
            return $"{FriendlyName} ({ActualValue})";
        }
    }
}
