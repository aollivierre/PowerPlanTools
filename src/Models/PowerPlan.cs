using System;
using System.Collections.Generic;
using System.Management.Automation;

namespace PowerPlanTools.Models
{
    /// <summary>
    /// Represents a Windows power plan
    /// </summary>
    public class PowerPlan
    {
        /// <summary>
        /// Gets or sets the name of the power plan
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the GUID of the power plan
        /// </summary>
        public Guid Guid { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this power plan is active
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Gets or sets the description of the power plan
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the list of power settings associated with this power plan
        /// </summary>
        public List<PowerSetting> Settings { get; set; }

        /// <summary>
        /// Initializes a new instance of the PowerPlan class
        /// </summary>
        public PowerPlan()
        {
            Settings = new List<PowerSetting>();
        }

        /// <summary>
        /// Initializes a new instance of the PowerPlan class with specified values
        /// </summary>
        /// <param name="name">The name of the power plan</param>
        /// <param name="guid">The GUID of the power plan</param>
        /// <param name="isActive">A value indicating whether this power plan is active</param>
        /// <param name="description">The description of the power plan</param>
        public PowerPlan(string name, Guid guid, bool isActive, string description)
        {
            Name = name;
            Guid = guid;
            IsActive = isActive;
            Description = description;
            Settings = new List<PowerSetting>();
        }

        /// <summary>
        /// Returns a string that represents the current object
        /// </summary>
        /// <returns>A string that represents the current object</returns>
        public override string ToString()
        {
            return $"{Name} ({Guid})";
        }
    }
}
