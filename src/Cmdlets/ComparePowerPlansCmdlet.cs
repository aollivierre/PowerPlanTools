using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using PowerPlanTools.Models;
using PowerPlanTools.Utils;

namespace PowerPlanTools.Cmdlets
{
    /// <summary>
    /// <para type="synopsis">Compares two power plans and shows the differences.</para>
    /// <para type="description">The Compare-PowerPlans cmdlet compares two Windows power plans and shows the differences in their settings.</para>
    /// <para type="description">You can specify plans by name or GUID.</para>
    /// <example>
    ///     <para>Compare two power plans by name</para>
    ///     <code>Compare-PowerPlans -ReferencePlanName "Balanced" -DifferencePlanName "Power saver"</code>
    /// </example>
    /// <example>
    ///     <para>Compare two power plans by GUID</para>
    ///     <code>Compare-PowerPlans -ReferencePlanGuid "381b4222-f694-41f0-9685-ff5bb260df2e" -DifferencePlanGuid "a1841308-3541-4fab-bc81-f71556f20b4a"</code>
    /// </example>
    /// <example>
    ///     <para>Compare two power plans including hidden settings</para>
    ///     <code>Compare-PowerPlans -ReferencePlanName "Balanced" -DifferencePlanName "Power saver" -IncludeHidden</code>
    /// </example>
    /// </summary>
    [Cmdlet(VerbsData.Compare, "PowerPlans")]
    [OutputType(typeof(PowerSettingDifference))]
    public class ComparePowerPlansCmdlet : PSCmdlet
    {
        /// <summary>
        /// <para type="description">Gets or sets the name of the reference power plan.</para>
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipeline = true, ValueFromPipelineByPropertyName = true, ParameterSetName = "ByPlanName")]
        [ValidateNotNullOrEmpty]
        public string ReferencePlanName { get; set; }

        /// <summary>
        /// <para type="description">Gets or sets the GUID of the reference power plan.</para>
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = true, ParameterSetName = "ByPlanGuid")]
        public Guid ReferencePlanGuid { get; set; }

        /// <summary>
        /// <para type="description">Gets or sets the name of the difference power plan.</para>
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = true, ParameterSetName = "ByPlanName")]
        [ValidateNotNullOrEmpty]
        public string DifferencePlanName { get; set; }

        /// <summary>
        /// <para type="description">Gets or sets the GUID of the difference power plan.</para>
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = true, ParameterSetName = "ByPlanGuid")]
        public Guid DifferencePlanGuid { get; set; }

        /// <summary>
        /// <para type="description">Gets or sets whether to include hidden settings in the comparison.</para>
        /// </summary>
        [Parameter]
        public SwitchParameter IncludeHidden { get; set; }

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
                // Get the reference plan GUID and name
                Guid referencePlanGuid;
                string referencePlanName;

                if (ParameterSetName == "ByPlanName")
                {
                    // Find the plan by name
                    var powerPlans = UsePowrProf ? PowerProfileHelper.GetPowerPlans() : WmiHelper.GetPowerPlans();
                    var plan = powerPlans.Find(p => p.Name.Equals(ReferencePlanName, StringComparison.OrdinalIgnoreCase));

                    if (plan == null)
                    {
                        WriteError(new ErrorRecord(
                            new ArgumentException($"Reference power plan '{ReferencePlanName}' not found."),
                            "ReferencePowerPlanNotFound",
                            ErrorCategory.ObjectNotFound,
                            ReferencePlanName));
                        return;
                    }

                    referencePlanGuid = plan.Guid;
                    referencePlanName = plan.Name;
                }
                else
                {
                    // Use the provided GUID
                    referencePlanGuid = ReferencePlanGuid;

                    // Get the plan name for display
                    var powerPlans = UsePowrProf ? PowerProfileHelper.GetPowerPlans() : WmiHelper.GetPowerPlans();
                    var plan = powerPlans.Find(p => p.Guid == referencePlanGuid);
                    referencePlanName = plan?.Name ?? referencePlanGuid.ToString();
                }

                // Get the difference plan GUID and name
                Guid differencePlanGuid;
                string differencePlanName;

                if (ParameterSetName == "ByPlanName")
                {
                    // Find the plan by name
                    var powerPlans = UsePowrProf ? PowerProfileHelper.GetPowerPlans() : WmiHelper.GetPowerPlans();
                    var plan = powerPlans.Find(p => p.Name.Equals(DifferencePlanName, StringComparison.OrdinalIgnoreCase));

                    if (plan == null)
                    {
                        WriteError(new ErrorRecord(
                            new ArgumentException($"Difference power plan '{DifferencePlanName}' not found."),
                            "DifferencePowerPlanNotFound",
                            ErrorCategory.ObjectNotFound,
                            DifferencePlanName));
                        return;
                    }

                    differencePlanGuid = plan.Guid;
                    differencePlanName = plan.Name;
                }
                else
                {
                    // Use the provided GUID
                    differencePlanGuid = DifferencePlanGuid;

                    // Get the plan name for display
                    var powerPlans = UsePowrProf ? PowerProfileHelper.GetPowerPlans() : WmiHelper.GetPowerPlans();
                    var plan = powerPlans.Find(p => p.Guid == differencePlanGuid);
                    differencePlanName = plan?.Name ?? differencePlanGuid.ToString();
                }

                // Get power settings for both plans
                var referenceSettings = UsePowrProf ?
                    PowerProfileHelper.GetPowerSettings(referencePlanGuid, IncludeHidden) :
                    WmiHelper.GetPowerSettings(referencePlanGuid, IncludeHidden);

                var differenceSettings = UsePowrProf ?
                    PowerProfileHelper.GetPowerSettings(differencePlanGuid, IncludeHidden) :
                    WmiHelper.GetPowerSettings(differencePlanGuid, IncludeHidden);

                // Compare settings
                var differences = ComparePowerSettings(referenceSettings, differenceSettings, referencePlanName, differencePlanName);

                // Write output
                foreach (var difference in differences)
                {
                    WriteObject(difference);
                }

                // Write summary
                LoggingHelper.LogVerbose(this, $"Found {differences.Count} differences between '{referencePlanName}' and '{differencePlanName}'.");
            }
            catch (Exception ex)
            {
                WriteError(new ErrorRecord(ex, "ComparePowerPlansError", ErrorCategory.NotSpecified, null));
            }
        }

        /// <summary>
        /// Compares power settings between two plans.
        /// </summary>
        /// <param name="referenceSettings">The reference plan settings.</param>
        /// <param name="differenceSettings">The difference plan settings.</param>
        /// <param name="referencePlanName">The reference plan name.</param>
        /// <param name="differencePlanName">The difference plan name.</param>
        /// <returns>A list of differences.</returns>
        private List<PowerSettingDifference> ComparePowerSettings(
            List<PowerSetting> referenceSettings,
            List<PowerSetting> differenceSettings,
            string referencePlanName,
            string differencePlanName)
        {
            List<PowerSettingDifference> differences = new List<PowerSettingDifference>();

            // Create dictionaries for faster lookup
            Dictionary<Guid, PowerSetting> referenceDict = referenceSettings.ToDictionary(s => s.SettingGuid);
            Dictionary<Guid, PowerSetting> differenceDict = differenceSettings.ToDictionary(s => s.SettingGuid);

            // Check settings in reference plan
            foreach (var referenceSetting in referenceSettings)
            {
                if (differenceDict.TryGetValue(referenceSetting.SettingGuid, out PowerSetting differenceSetting))
                {
                    // Setting exists in both plans, check for differences
                    bool acDifferent = !Equals(referenceSetting.PluggedIn, differenceSetting.PluggedIn);
                    bool dcDifferent = !Equals(referenceSetting.OnBattery, differenceSetting.OnBattery);

                    if (acDifferent || dcDifferent)
                    {
                        differences.Add(new PowerSettingDifference
                        {
                            SettingAlias = referenceSetting.Alias,
                            SettingGuid = referenceSetting.SettingGuid,
                            ReferencePlanName = referencePlanName,
                            DifferencePlanName = differencePlanName,
                            ReferencePluggedIn = referenceSetting.PluggedIn,
                            DifferencePluggedIn = differenceSetting.PluggedIn,
                            ReferenceOnBattery = referenceSetting.OnBattery,
                            DifferenceOnBattery = differenceSetting.OnBattery,
                            Units = referenceSetting.Units,
                            Description = referenceSetting.Description
                        });
                    }
                }
                else
                {
                    // Setting exists only in reference plan
                    differences.Add(new PowerSettingDifference
                    {
                        SettingAlias = referenceSetting.Alias,
                        SettingGuid = referenceSetting.SettingGuid,
                        ReferencePlanName = referencePlanName,
                        DifferencePlanName = differencePlanName,
                        ReferencePluggedIn = referenceSetting.PluggedIn,
                        DifferencePluggedIn = null,
                        ReferenceOnBattery = referenceSetting.OnBattery,
                        DifferenceOnBattery = null,
                        Units = referenceSetting.Units,
                        Description = referenceSetting.Description,
                        OnlyInReferencePlan = true
                    });
                }
            }

            // Check for settings only in difference plan
            foreach (var differenceSetting in differenceSettings)
            {
                if (!referenceDict.ContainsKey(differenceSetting.SettingGuid))
                {
                    // Setting exists only in difference plan
                    differences.Add(new PowerSettingDifference
                    {
                        SettingAlias = differenceSetting.Alias,
                        SettingGuid = differenceSetting.SettingGuid,
                        ReferencePlanName = referencePlanName,
                        DifferencePlanName = differencePlanName,
                        ReferencePluggedIn = null,
                        DifferencePluggedIn = differenceSetting.PluggedIn,
                        ReferenceOnBattery = null,
                        DifferenceOnBattery = differenceSetting.OnBattery,
                        Units = differenceSetting.Units,
                        Description = differenceSetting.Description,
                        OnlyInDifferencePlan = true
                    });
                }
            }

            return differences;
        }
    }

    /// <summary>
    /// Represents a difference between power settings in two plans.
    /// </summary>
    public class PowerSettingDifference
    {
        /// <summary>
        /// Gets or sets the alias of the setting.
        /// </summary>
        public string SettingAlias { get; set; }

        /// <summary>
        /// Gets or sets the GUID of the setting.
        /// </summary>
        public Guid SettingGuid { get; set; }

        /// <summary>
        /// Gets or sets the name of the reference plan.
        /// </summary>
        public string ReferencePlanName { get; set; }

        /// <summary>
        /// Gets or sets the name of the difference plan.
        /// </summary>
        public string DifferencePlanName { get; set; }

        /// <summary>
        /// Gets or sets the plugged-in value in the reference plan.
        /// </summary>
        public object ReferencePluggedIn { get; set; }

        /// <summary>
        /// Gets or sets the plugged-in value in the difference plan.
        /// </summary>
        public object DifferencePluggedIn { get; set; }

        /// <summary>
        /// Gets or sets the on-battery value in the reference plan.
        /// </summary>
        public object ReferenceOnBattery { get; set; }

        /// <summary>
        /// Gets or sets the on-battery value in the difference plan.
        /// </summary>
        public object DifferenceOnBattery { get; set; }

        /// <summary>
        /// Gets or sets the units for the setting values.
        /// </summary>
        public string Units { get; set; }

        /// <summary>
        /// Gets or sets the description of the setting.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets whether the setting exists only in the reference plan.
        /// </summary>
        public bool OnlyInReferencePlan { get; set; }

        /// <summary>
        /// Gets or sets whether the setting exists only in the difference plan.
        /// </summary>
        public bool OnlyInDifferencePlan { get; set; }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString()
        {
            if (OnlyInReferencePlan)
            {
                return $"{SettingAlias} - Only in {ReferencePlanName}";
            }
            else if (OnlyInDifferencePlan)
            {
                return $"{SettingAlias} - Only in {DifferencePlanName}";
            }
            else
            {
                return $"{SettingAlias} - Different values";
            }
        }
    }
}
