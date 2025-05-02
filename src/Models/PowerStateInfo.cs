using System;

namespace PowerPlanTools.Models
{
    /// <summary>
    /// Represents information about various power state settings.
    /// </summary>
    public class PowerStateInfo
    {
        /// <summary>
        /// Gets or sets whether Connected Standby (Modern Standby) is enabled.
        /// </summary>
        public bool ConnectedStandby { get; set; }

        /// <summary>
        /// Gets or sets whether Fast Startup is enabled.
        /// </summary>
        public bool FastStartup { get; set; }

        /// <summary>
        /// Gets or sets whether S0 Low Power Idle is enabled.
        /// </summary>
        public bool S0LowPowerIdle { get; set; }

        /// <summary>
        /// Gets or sets whether Hibernate is enabled.
        /// </summary>
        public bool Hibernate { get; set; }

        /// <summary>
        /// Gets or sets whether Hybrid Sleep is enabled.
        /// </summary>
        public bool HybridSleep { get; set; }
    }
}
