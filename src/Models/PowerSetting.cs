using System;

namespace PowerPlanTools.Models
{
    /// <summary>
    /// Represents a power setting within a power plan
    /// </summary>
    public class PowerSetting
    {
        /// <summary>
        /// Gets or sets the friendly alias for the setting
        /// </summary>
        public string Alias { get; set; }

        /// <summary>
        /// Gets or sets the GUID of the setting
        /// </summary>
        public Guid SettingGuid { get; set; }

        /// <summary>
        /// Gets or sets the GUID of the subgroup containing this setting
        /// </summary>
        public Guid SubGroupGuid { get; set; }

        /// <summary>
        /// Gets or sets the value when on battery power
        /// </summary>
        public object OnBattery { get; set; }

        /// <summary>
        /// Gets or sets the value when plugged in
        /// </summary>
        public object PluggedIn { get; set; }

        /// <summary>
        /// Gets or sets the units for the setting values
        /// </summary>
        public string Units { get; set; }

        /// <summary>
        /// Gets or sets the description of the setting
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this setting is hidden
        /// </summary>
        public bool IsHidden { get; set; }

        /// <summary>
        /// Gets or sets the minimum possible value for this setting
        /// </summary>
        public object MinValue { get; set; }

        /// <summary>
        /// Gets or sets the maximum possible value for this setting
        /// </summary>
        public object MaxValue { get; set; }

        /// <summary>
        /// Gets or sets the possible values for this setting (if enumeration)
        /// </summary>
        public object[] PossibleValues { get; set; }

        /// <summary>
        /// Gets or sets the name of the power plan this setting belongs to
        /// </summary>
        public string PlanName { get; set; }

        /// <summary>
        /// Gets or sets the GUID of the power plan this setting belongs to
        /// </summary>
        public Guid PlanGuid { get; set; }

        /// <summary>
        /// Initializes a new instance of the PowerSetting class
        /// </summary>
        public PowerSetting()
        {
        }

        /// <summary>
        /// Initializes a new instance of the PowerSetting class with specified values
        /// </summary>
        /// <param name="alias">The friendly alias for the setting</param>
        /// <param name="settingGuid">The GUID of the setting</param>
        /// <param name="subGroupGuid">The GUID of the subgroup containing this setting</param>
        /// <param name="onBattery">The value when on battery power</param>
        /// <param name="pluggedIn">The value when plugged in</param>
        /// <param name="units">The units for the setting values</param>
        /// <param name="description">The description of the setting</param>
        public PowerSetting(string alias, Guid settingGuid, Guid subGroupGuid, object onBattery, object pluggedIn, string units, string description)
        {
            Alias = alias;
            SettingGuid = settingGuid;
            SubGroupGuid = subGroupGuid;
            OnBattery = onBattery;
            PluggedIn = pluggedIn;
            Units = units;
            Description = description;
            IsHidden = false;
        }

        /// <summary>
        /// Returns a string that represents the current object
        /// </summary>
        /// <returns>A string that represents the current object</returns>
        public override string ToString()
        {
            if (!string.IsNullOrEmpty(PlanName))
            {
                return $"{Alias} ({SettingGuid}) - Plan: {PlanName}";
            }
            else
            {
                return $"{Alias} ({SettingGuid})";
            }
        }
    }
}
