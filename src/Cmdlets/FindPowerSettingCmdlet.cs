using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text.RegularExpressions;
using PowerPlanTools.Models;
using PowerPlanTools.Utils;

namespace PowerPlanTools.Cmdlets
{
    /// <summary>
    /// <para type="synopsis">Finds power settings based on search criteria.</para>
    /// <para type="description">The Find-PowerSetting cmdlet searches for power settings across all power plans or within a specific plan.</para>
    /// <para type="description">You can search by name, description, or GUID pattern using simple text, regex, or wildcard patterns.</para>
    /// <example>
    ///     <para>Find all power settings containing "display" in their name or description</para>
    ///     <code>Find-PowerSetting -SearchString "display"</code>
    /// </example>
    /// <example>
    ///     <para>Find power settings in a specific plan containing "processor" in their name</para>
    ///     <code>Find-PowerSetting -PlanName "Balanced" -SearchString "processor" -SearchIn Name</code>
    /// </example>
    /// <example>
    ///     <para>Find power settings matching a GUID pattern</para>
    ///     <code>Find-PowerSetting -GuidPattern "7516b95f"</code>
    /// </example>
    /// <example>
    ///     <para>Find all hidden power settings</para>
    ///     <code>Find-PowerSetting -Hidden</code>
    /// </example>
    /// <example>
    ///     <para>Find power settings using a regex pattern</para>
    ///     <code>Find-PowerSetting -SearchString "^processor.*state$" -Regex</code>
    /// </example>
    /// <example>
    ///     <para>Find power settings using a wildcard pattern</para>
    ///     <code>Find-PowerSetting -SearchString "USB*suspend*" -Wildcard</code>
    /// </example>
    /// </summary>
    [Cmdlet(VerbsCommon.Find, "PowerSetting")]
    [OutputType(typeof(PowerSetting))]
    public class FindPowerSettingCmdlet : PSCmdlet
    {
        /// <summary>
        /// <para type="description">Gets or sets the name of the power plan to search in.</para>
        /// </summary>
        [Parameter(ValueFromPipeline = true, ValueFromPipelineByPropertyName = true)]
        [ArgumentCompleter(typeof(ArgumentCompleters.PowerPlanNameCompleter))]
        public string PlanName { get; set; }

        /// <summary>
        /// <para type="description">Gets or sets the GUID of the power plan to search in.</para>
        /// </summary>
        [Parameter(ValueFromPipelineByPropertyName = true)]
        [ArgumentCompleter(typeof(ArgumentCompleters.PowerPlanGuidCompleter))]
        public Guid? PlanGuid { get; set; }

        /// <summary>
        /// <para type="description">Gets or sets the search string to find in power setting names or descriptions.</para>
        /// </summary>
        [Parameter(Position = 0)]
        public string SearchString { get; set; }

        /// <summary>
        /// <para type="description">Gets or sets whether to use regex pattern matching for the search string.</para>
        /// </summary>
        [Parameter]
        public SwitchParameter Regex { get; set; }

        /// <summary>
        /// <para type="description">Gets or sets whether to use wildcard pattern matching for the search string.</para>
        /// </summary>
        [Parameter]
        public SwitchParameter Wildcard { get; set; }

        /// <summary>
        /// <para type="description">Gets or sets the GUID pattern to search for.</para>
        /// </summary>
        [Parameter]
        public string GuidPattern { get; set; }

        /// <summary>
        /// <para type="description">Gets or sets where to search for the search string.</para>
        /// </summary>
        [Parameter]
        [ValidateSet("Name", "Description", "Both")]
        public string SearchIn { get; set; } = "Both";

        /// <summary>
        /// <para type="description">Gets or sets whether to search for hidden settings only.</para>
        /// </summary>
        [Parameter]
        public SwitchParameter Hidden { get; set; }

        /// <summary>
        /// <para type="description">Gets or sets whether to include hidden settings in the search results.</para>
        /// </summary>
        [Parameter]
        public SwitchParameter IncludeHidden { get; set; }





        /// <summary>
        /// Processes the cmdlet.
        /// </summary>
        protected override void ProcessRecord()
        {
            try
            {
                // Get all power plans or a specific plan
                List<PowerPlan> powerPlans = PowerProfileHelper.GetPowerPlans();

                // Filter by plan name or GUID if specified
                if (!string.IsNullOrEmpty(PlanName))
                {
                    powerPlans = powerPlans.Where(p => p.Name.Equals(PlanName, StringComparison.OrdinalIgnoreCase)).ToList();

                    if (powerPlans.Count == 0)
                    {
                        WriteError(new ErrorRecord(
                            new ArgumentException($"Power plan '{PlanName}' not found."),
                            "PowerPlanNotFound",
                            ErrorCategory.ObjectNotFound,
                            PlanName));
                        return;
                    }
                }
                else if (PlanGuid.HasValue)
                {
                    powerPlans = powerPlans.Where(p => p.Guid == PlanGuid.Value).ToList();

                    if (powerPlans.Count == 0)
                    {
                        WriteError(new ErrorRecord(
                            new ArgumentException($"Power plan with GUID '{PlanGuid}' not found."),
                            "PowerPlanNotFound",
                            ErrorCategory.ObjectNotFound,
                            PlanGuid));
                        return;
                    }
                }

                // Get all settings from the selected plans
                List<PowerSetting> allSettings = new List<PowerSetting>();

                foreach (var plan in powerPlans)
                {
                    List<PowerSetting> planSettings = PowerProfileHelper.GetPowerSettings(plan.Guid, true);

                    // Add plan information to each setting
                    foreach (var setting in planSettings)
                    {
                        setting.PlanName = plan.Name;
                        setting.PlanGuid = plan.Guid;
                    }

                    allSettings.AddRange(planSettings);
                }

                // Filter settings based on search criteria
                IEnumerable<PowerSetting> filteredSettings = allSettings;

                // Filter by hidden status
                if (Hidden.IsPresent)
                {
                    filteredSettings = filteredSettings.Where(s => s.IsHidden);
                }
                else if (!IncludeHidden.IsPresent && !Hidden.IsPresent)
                {
                    filteredSettings = filteredSettings.Where(s => !s.IsHidden);
                }

                // Filter by search string
                if (!string.IsNullOrEmpty(SearchString))
                {
                    // Determine the search method to use
                    Func<string, string, bool> matchFunc;

                    if (Regex.IsPresent)
                    {
                        // Use regex pattern matching (case-insensitive)
                        Regex regex = new Regex(SearchString, RegexOptions.IgnoreCase);
                        matchFunc = (text, pattern) => text != null && regex.IsMatch(text);
                    }
                    else if (Wildcard.IsPresent)
                    {
                        // Use wildcard pattern matching (case-insensitive)
                        WildcardPattern wildcardPattern = new WildcardPattern(SearchString, WildcardOptions.IgnoreCase);
                        matchFunc = (text, pattern) => text != null && wildcardPattern.IsMatch(text);
                    }
                    else
                    {
                        // Use simple string contains (case-insensitive)
                        matchFunc = (text, pattern) => text != null && text.IndexOf(pattern, StringComparison.OrdinalIgnoreCase) >= 0;
                    }

                    // Apply the search based on the SearchIn parameter
                    switch (SearchIn.ToLowerInvariant())
                    {
                        case "name":
                            filteredSettings = filteredSettings.Where(s => matchFunc(s.Alias, SearchString));
                            break;
                        case "description":
                            filteredSettings = filteredSettings.Where(s => matchFunc(s.Description, SearchString));
                            break;
                        case "both":
                        default:
                            filteredSettings = filteredSettings.Where(s =>
                                matchFunc(s.Alias, SearchString) || matchFunc(s.Description, SearchString));
                            break;
                    }
                }

                // Filter by GUID pattern
                if (!string.IsNullOrEmpty(GuidPattern))
                {
                    // Determine the search method to use
                    Func<string, string, bool> matchFunc;

                    if (Regex.IsPresent)
                    {
                        // Use regex pattern matching (case-insensitive)
                        Regex regex = new Regex(GuidPattern, RegexOptions.IgnoreCase);
                        matchFunc = (text, pattern) => text != null && regex.IsMatch(text);
                    }
                    else if (Wildcard.IsPresent)
                    {
                        // Use wildcard pattern matching (case-insensitive)
                        WildcardPattern wildcardPattern = new WildcardPattern(GuidPattern, WildcardOptions.IgnoreCase);
                        matchFunc = (text, pattern) => text != null && wildcardPattern.IsMatch(text);
                    }
                    else
                    {
                        // Use simple string contains (case-insensitive)
                        matchFunc = (text, pattern) => text != null && text.IndexOf(pattern, StringComparison.OrdinalIgnoreCase) >= 0;
                    }

                    filteredSettings = filteredSettings.Where(s => matchFunc(s.SettingGuid.ToString(), GuidPattern));
                }

                // Remove duplicates (same setting GUID across different plans)
                filteredSettings = filteredSettings
                    .GroupBy(s => s.SettingGuid)
                    .Select(g => g.First())
                    .OrderBy(s => s.Alias);

                // Write output
                foreach (var setting in filteredSettings)
                {
                    // Add known alias if available
                    if (string.IsNullOrEmpty(setting.Alias) || setting.Alias == setting.SettingGuid.ToString())
                    {
                        string knownAlias = ArgumentCompleters.GetPowerSettingAlias(setting.SettingGuid);
                        if (knownAlias != setting.SettingGuid.ToString())
                        {
                            setting.Alias = knownAlias;
                        }
                    }

                    // Add subgroup alias if not already set
                    if (string.IsNullOrEmpty(setting.SubGroupAlias))
                    {
                        setting.SubGroupAlias = ArgumentCompleters.GetSubgroupAlias(setting.SubGroupGuid);
                    }

                    // Always add possible values
                    setting.PossibleValues = PowerProfileHelper.GetPowerSettingPossibleValues(setting.SettingGuid, setting.SubGroupGuid);

                    WriteObject(setting);
                }

                // Write summary
                LoggingHelper.LogVerbose(this, $"Found {filteredSettings.Count()} power settings matching the search criteria.");
            }
            catch (Exception ex)
            {
                WriteError(new ErrorRecord(ex, "FindPowerSettingError", ErrorCategory.NotSpecified, null));
            }
        }
    }
}
