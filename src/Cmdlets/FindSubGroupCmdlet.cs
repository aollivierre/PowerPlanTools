using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text.RegularExpressions;
using PowerPlanTools.Utils;

namespace PowerPlanTools.Cmdlets
{
    /// <summary>
    /// <para type="synopsis">Finds power subgroups by name or GUID.</para>
    /// <para type="description">Finds power subgroups by name or GUID. Supports wildcards and regex patterns.</para>
    /// <para type="description">This cmdlet is useful for finding the correct subgroup to use with other cmdlets.</para>
    /// <example>
    ///   <para>Find all subgroups with "power" in the name</para>
    ///   <code>Find-SubGroup -Name "*power*"</code>
    /// </example>
    /// <example>
    ///   <para>Find a subgroup by GUID</para>
    ///   <code>Find-SubGroup -SubGroupGuid "54533251-82be-4824-96c1-47b60b740d00"</code>
    /// </example>
    /// </summary>
    [Cmdlet(VerbsCommon.Find, "SubGroup")]
    [OutputType(typeof(SubGroupInfo))]
    public class FindSubGroupCmdlet : PSCmdlet
    {
        /// <summary>
        /// <para type="description">The name pattern to search for. Supports wildcards and regex patterns.</para>
        /// </summary>
        [Parameter(Position = 0, ParameterSetName = "ByName")]
        public string Name { get; set; }

        /// <summary>
        /// <para type="description">The GUID pattern to search for. Supports wildcards and regex patterns.</para>
        /// </summary>
        [Parameter(Position = 0, ParameterSetName = "ByGuid")]
        public string SubGroupGuid { get; set; }

        /// <summary>
        /// <para type="description">Use regex pattern matching instead of wildcards.</para>
        /// </summary>
        [Parameter()]
        public SwitchParameter Regex { get; set; }

        /// <summary>
        /// Process the cmdlet
        /// </summary>
        protected override void ProcessRecord()
        {
            try
            {
                // Get all known subgroups
                var subgroups = GetAllSubGroups();

                // Filter by name or GUID
                if (!string.IsNullOrEmpty(Name))
                {
                    subgroups = FilterByName(subgroups, Name);
                }
                else if (!string.IsNullOrEmpty(SubGroupGuid))
                {
                    subgroups = FilterByGuid(subgroups, SubGroupGuid);
                }

                // Write the results
                foreach (var subgroup in subgroups)
                {
                    WriteObject(subgroup);
                }
            }
            catch (Exception ex)
            {
                ThrowTerminatingError(new ErrorRecord(ex, "FindSubGroupError", ErrorCategory.NotSpecified, null));
            }
        }

        /// <summary>
        /// Gets all known subgroups
        /// </summary>
        /// <returns>A list of subgroup info objects</returns>
        private List<SubGroupInfo> GetAllSubGroups()
        {
            var result = new List<SubGroupInfo>();

            // Get all known subgroups from the ArgumentCompleters class
            var subgroups = typeof(ArgumentCompleters)
                .GetField("_knownSubgroups", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)
                .GetValue(null) as Dictionary<Guid, string>;

            if (subgroups != null)
            {
                foreach (var kvp in subgroups)
                {
                    result.Add(new SubGroupInfo
                    {
                        SubGroupGuid = kvp.Key,
                        Name = kvp.Value
                    });
                }
            }

            return result;
        }

        /// <summary>
        /// Filters subgroups by name
        /// </summary>
        /// <param name="subgroups">The subgroups to filter</param>
        /// <param name="pattern">The name pattern</param>
        /// <returns>The filtered subgroups</returns>
        private List<SubGroupInfo> FilterByName(List<SubGroupInfo> subgroups, string pattern)
        {
            if (Regex)
            {
                var regex = new Regex(pattern, RegexOptions.IgnoreCase);
                return subgroups.Where(s => regex.IsMatch(s.Name)).ToList();
            }
            else
            {
                WildcardPattern wildcard = new WildcardPattern(pattern, WildcardOptions.IgnoreCase);
                return subgroups.Where(s => wildcard.IsMatch(s.Name)).ToList();
            }
        }

        /// <summary>
        /// Filters subgroups by GUID
        /// </summary>
        /// <param name="subgroups">The subgroups to filter</param>
        /// <param name="pattern">The GUID pattern</param>
        /// <returns>The filtered subgroups</returns>
        private List<SubGroupInfo> FilterByGuid(List<SubGroupInfo> subgroups, string pattern)
        {
            if (Regex)
            {
                var regex = new Regex(pattern, RegexOptions.IgnoreCase);
                return subgroups.Where(s => regex.IsMatch(s.SubGroupGuid.ToString())).ToList();
            }
            else
            {
                WildcardPattern wildcard = new WildcardPattern(pattern, WildcardOptions.IgnoreCase);
                return subgroups.Where(s => wildcard.IsMatch(s.SubGroupGuid.ToString())).ToList();
            }
        }
    }

    /// <summary>
    /// Represents a power subgroup
    /// </summary>
    public class SubGroupInfo
    {
        /// <summary>
        /// The subgroup GUID
        /// </summary>
        public Guid SubGroupGuid { get; set; }

        /// <summary>
        /// The subgroup name
        /// </summary>
        public string Name { get; set; }
    }
}
