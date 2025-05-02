using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Text.RegularExpressions;
using System.Xml.Serialization;
using Newtonsoft.Json;
using PowerPlanTools.Models;
using PowerPlanTools.Utils;

namespace PowerPlanTools.Cmdlets
{
    /// <summary>
    /// <para type="synopsis">Imports power settings from a file.</para>
    /// <para type="description">The Import-PowerSettings cmdlet imports power settings from a file and applies them to a Windows power plan.</para>
    /// <para type="description">You can import from JSON, CSV, or XML files previously exported with Export-PowerSettings.</para>
    /// <example>
    ///     <para>Import power settings from a JSON file</para>
    ///     <code>Import-PowerSettings -Path "C:\Reports\BalancedPlan.json" -PlanName "Balanced"</code>
    /// </example>
    /// <example>
    ///     <para>Import power settings from a CSV file to a specific plan</para>
    ///     <code>Import-PowerSettings -Path "C:\Reports\BalancedPlan.csv" -PlanGuid "381b4222-f694-41f0-9685-ff5bb260df2e"</code>
    /// </example>
    /// <example>
    ///     <para>Import power settings and create a new plan if it doesn't exist</para>
    ///     <code>Import-PowerSettings -Path "C:\Reports\CustomPlan.xml" -PlanName "Custom Plan" -CreateIfNotExists</code>
    /// </example>
    /// <example>
    ///     <para>Import power settings and preview changes without applying them</para>
    ///     <code>Import-PowerSettings -Path "C:\Reports\BalancedPlan.json" -PlanName "Balanced" -WhatIf</code>
    /// </example>
    /// </summary>
    [Cmdlet(VerbsData.Import, "PowerSettings", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.High)]
    [OutputType(typeof(PowerPlan))]
    public class ImportPowerSettingsCmdlet : PSCmdlet
    {

        /// <summary>
        /// <para type="description">Gets or sets the path to the input file.</para>
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ValueFromPipeline = true, ValueFromPipelineByPropertyName = true)]
        [ValidateNotNullOrEmpty]
        public string Path { get; set; }

        /// <summary>
        /// <para type="description">Gets or sets the name of the power plan to import settings to.</para>
        /// </summary>
        [Parameter(Mandatory = true, ParameterSetName = "ByPlanName")]
        [ValidateNotNullOrEmpty]
        [ArgumentCompleter(typeof(ArgumentCompleters.PowerPlanNameCompleter))]
        public string PlanName { get; set; }

        /// <summary>
        /// <para type="description">Gets or sets the GUID of the power plan to import settings to.</para>
        /// </summary>
        [Parameter(Mandatory = true, ParameterSetName = "ByPlanGuid")]
        [ArgumentCompleter(typeof(ArgumentCompleters.PowerPlanGuidCompleter))]
        public Guid PlanGuid { get; set; }

        /// <summary>
        /// <para type="description">Gets or sets whether to create the power plan if it doesn't exist.</para>
        /// </summary>
        [Parameter]
        public SwitchParameter CreateIfNotExists { get; set; }

        /// <summary>
        /// <para type="description">Gets or sets whether to use WMI instead of PowrProf.dll.</para>
        /// </summary>
        [Parameter]
        public SwitchParameter UseWmi { get; set; }

        /// <summary>
        /// <para type="description">Gets or sets whether to return the updated power plan.</para>
        /// </summary>
        [Parameter]
        public SwitchParameter PassThru { get; set; }

        /// <summary>
        /// <para type="description">Gets or sets whether to force the import without prompting for confirmation.</para>
        /// </summary>
        [Parameter]
        public SwitchParameter Force { get; set; }

        /// <summary>
        /// Processes the cmdlet.
        /// </summary>
        protected override void ProcessRecord()
        {
            try
            {
                LoggingHelper.LogVerbose(this, $"Starting import from file: {Path}");

                // Validate the file exists
                if (!File.Exists(Path))
                {
                    WriteError(new ErrorRecord(
                        new FileNotFoundException($"File not found: {Path}"),
                        "FileNotFound",
                        ErrorCategory.ObjectNotFound,
                        Path));
                    return;
                }

                // Determine the file format based on extension
                string extension = System.IO.Path.GetExtension(Path).ToLowerInvariant();
                LoggingHelper.LogVerbose(this, $"Detected file format: {extension}");
                PowerPlan importedPlan;

                switch (extension)
                {
                    case ".json":
                        LoggingHelper.LogVerbose(this, $"Importing from JSON file");
                        importedPlan = ImportFromJson(Path);
                        break;
                    case ".csv":
                        LoggingHelper.LogVerbose(this, $"Importing from CSV file");
                        importedPlan = ImportFromCsv(Path);
                        break;
                    case ".xml":
                        LoggingHelper.LogVerbose(this, $"Importing from XML file");
                        importedPlan = ImportFromXml(Path);
                        break;
                    default:
                        WriteError(new ErrorRecord(
                            new ArgumentException($"Unsupported file format: {extension}. Supported formats are .json, .csv, and .xml."),
                            "UnsupportedFormat",
                            ErrorCategory.InvalidArgument,
                            extension));
                        return;
                }

                if (importedPlan == null || importedPlan.Settings == null || importedPlan.Settings.Count == 0)
                {
                    WriteError(new ErrorRecord(
                        new InvalidOperationException($"No valid power settings found in the file: {Path}"),
                        "NoSettingsFound",
                        ErrorCategory.InvalidData,
                        Path));
                    return;
                }

                LoggingHelper.LogVerbose(this, $"Successfully imported plan '{importedPlan.Name}' with {importedPlan.Settings.Count} settings");

                // Get the target power plan
                Guid targetPlanGuid;
                string targetPlanName;
                bool planExists = true;

                if (ParameterSetName == "ByPlanName")
                {
                    // Find the plan by name
                    var powerPlans = UseWmi ? WmiHelper.GetPowerPlans() : PowerProfileHelper.GetPowerPlans();
                    var plan = powerPlans.Find(p => p.Name.Equals(PlanName, StringComparison.OrdinalIgnoreCase));

                    if (plan == null)
                    {
                        if (!CreateIfNotExists)
                        {
                            WriteError(new ErrorRecord(
                                new ArgumentException($"Power plan '{PlanName}' not found. Use -CreateIfNotExists to create it."),
                                "PowerPlanNotFound",
                                ErrorCategory.ObjectNotFound,
                                PlanName));
                            return;
                        }

                        // Create the plan by cloning the Balanced plan
                        LoggingHelper.LogVerbose(this, $"Power plan '{PlanName}' not found. Attempting to create it.");
                        var balancedPlan = powerPlans.Find(p => p.Name.Equals("Balanced", StringComparison.OrdinalIgnoreCase));
                        if (balancedPlan == null)
                        {
                            WriteError(new ErrorRecord(
                                new InvalidOperationException("Cannot create new plan: Balanced plan not found."),
                                "BalancedPlanNotFound",
                                ErrorCategory.ObjectNotFound,
                                "Balanced"));
                            return;
                        }

                        LoggingHelper.LogVerbose(this, $"Found Balanced plan ({balancedPlan.Guid}) to use as template");
                        if (!ShouldProcess($"Create new power plan '{PlanName}'", "Import-PowerSettings"))
                        {
                            return;
                        }

                        LoggingHelper.LogVerbose(this, $"Creating new power plan: {PlanName}");
                        if (UseWmi)
                        {
                            LoggingHelper.LogVerbose(this, $"Using WmiHelper to create plan");
                            targetPlanGuid = WmiHelper.CreatePowerPlan(balancedPlan.Guid, PlanName);
                        }
                        else
                        {
                            LoggingHelper.LogVerbose(this, $"Using PowerProfileHelper to create plan");
                            targetPlanGuid = PowerProfileHelper.CreatePowerPlan(balancedPlan.Guid, PlanName);
                        }

                        if (targetPlanGuid == Guid.Empty)
                        {
                            WriteError(new ErrorRecord(
                                new InvalidOperationException($"Failed to create power plan '{PlanName}'."),
                                "CreatePowerPlanFailed",
                                ErrorCategory.InvalidOperation,
                                PlanName));
                            return;
                        }

                        LoggingHelper.LogVerbose(this, $"Successfully created new power plan: {PlanName} ({targetPlanGuid})");
                        targetPlanName = PlanName;
                        planExists = false;
                    }
                    else
                    {
                        targetPlanGuid = plan.Guid;
                        targetPlanName = plan.Name;
                    }
                }
                else
                {
                    // Use the provided GUID
                    targetPlanGuid = PlanGuid;

                    // Get the plan name for display
                    var powerPlans = UseWmi ? WmiHelper.GetPowerPlans() : PowerProfileHelper.GetPowerPlans();
                    var plan = powerPlans.Find(p => p.Guid == targetPlanGuid);

                    if (plan == null)
                    {
                        if (!CreateIfNotExists)
                        {
                            WriteError(new ErrorRecord(
                                new ArgumentException($"Power plan with GUID '{targetPlanGuid}' not found. Use -CreateIfNotExists to create it."),
                                "PowerPlanNotFound",
                                ErrorCategory.ObjectNotFound,
                                targetPlanGuid));
                            return;
                        }

                        // Create the plan by cloning the Balanced plan
                        var balancedPlan = powerPlans.Find(p => p.Name.Equals("Balanced", StringComparison.OrdinalIgnoreCase));
                        if (balancedPlan == null)
                        {
                            WriteError(new ErrorRecord(
                                new InvalidOperationException("Cannot create new plan: Balanced plan not found."),
                                "BalancedPlanNotFound",
                                ErrorCategory.ObjectNotFound,
                                "Balanced"));
                            return;
                        }

                        string newPlanName = importedPlan.Name ?? $"Imported Plan {DateTime.Now:yyyyMMddHHmmss}";
                        LoggingHelper.LogVerbose(this, $"Creating new power plan: {newPlanName}");
                        if (!ShouldProcess($"Create new power plan '{newPlanName}'", "Import-PowerSettings"))
                        {
                            return;
                        }

                        if (UseWmi)
                        {
                            targetPlanGuid = WmiHelper.CreatePowerPlan(balancedPlan.Guid, newPlanName);
                        }
                        else
                        {
                            targetPlanGuid = PowerProfileHelper.CreatePowerPlan(balancedPlan.Guid, newPlanName);
                        }

                        if (targetPlanGuid == Guid.Empty)
                        {
                            WriteError(new ErrorRecord(
                                new InvalidOperationException($"Failed to create power plan '{newPlanName}'."),
                                "CreatePowerPlanFailed",
                                ErrorCategory.InvalidOperation,
                                newPlanName));
                            return;
                        }

                        targetPlanName = newPlanName;
                        planExists = false;
                    }
                    else
                    {
                        targetPlanName = plan.Name;
                    }
                }

                // Confirm the action
                if (!Force && !ShouldProcess($"Import {importedPlan.Settings.Count} power settings to plan '{targetPlanName}'", "Import-PowerSettings"))
                {
                    return;
                }

                // Apply the settings to the target plan
                int successCount = 0;
                int failureCount = 0;
                int totalSettings = importedPlan.Settings.Count;

                LoggingHelper.LogVerbose(this, $"Applying {totalSettings} settings to power plan '{targetPlanName}' ({targetPlanGuid})");

                for (int i = 0; i < importedPlan.Settings.Count; i++)
                {
                    var setting = importedPlan.Settings[i];
                    try
                    {
                        LoggingHelper.LogVerbose(this, $"Updating setting: {setting.Alias} ({setting.SettingGuid})", i + 1, totalSettings);
                        LoggingHelper.LogVerbose(this, $"  Values - OnBattery: {setting.OnBattery}, PluggedIn: {setting.PluggedIn}");

                        bool success;
                        if (UseWmi)
                        {
                            success = WmiHelper.UpdatePowerSetting(
                                targetPlanGuid,
                                setting.SettingGuid,
                                setting.SubGroupGuid,
                                setting.PluggedIn,
                                setting.OnBattery);
                        }
                        else
                        {
                            success = PowerProfileHelper.UpdatePowerSetting(
                                targetPlanGuid,
                                setting.SettingGuid,
                                setting.SubGroupGuid,
                                setting.PluggedIn != null ? Convert.ToUInt32(setting.PluggedIn) : (uint?)null,
                                setting.OnBattery != null ? Convert.ToUInt32(setting.OnBattery) : (uint?)null);
                        }

                        if (success)
                        {
                            successCount++;
                            LoggingHelper.LogVerbose(this, $"  Setting updated successfully", i + 1, totalSettings);
                        }
                        else
                        {
                            failureCount++;
                            string warningMsg = $"Failed to update setting: {setting.Alias} ({setting.SettingGuid})";
                            WriteWarning(warningMsg);
                            LoggingHelper.LogVerbose(this, $"  WARNING: {warningMsg}", i + 1, totalSettings);
                        }
                    }
                    catch (Exception ex)
                    {
                        failureCount++;
                        string errorMsg = $"Error updating setting {setting.Alias} ({setting.SettingGuid}): {ex.Message}";
                        WriteWarning(errorMsg);
                        LoggingHelper.LogVerbose(this, $"  ERROR: {errorMsg}", i + 1, totalSettings);
                    }
                }

                LoggingHelper.LogVerbose(this, $"Import summary: {successCount} settings successfully imported, {failureCount} failed");

                if (failureCount > 0)
                {
                    WriteWarning($"Failed to import {failureCount} settings.");
                }

                // Return the updated power plan if requested
                if (PassThru)
                {
                    var powerPlans = UseWmi ? WmiHelper.GetPowerPlans() : PowerProfileHelper.GetPowerPlans();
                    var updatedPlan = powerPlans.Find(p => p.Guid == targetPlanGuid);
                    if (updatedPlan != null)
                    {
                        if (UseWmi)
                        {
                            updatedPlan.Settings = WmiHelper.GetPowerSettings(targetPlanGuid, true);
                        }
                        else
                        {
                            updatedPlan.Settings = PowerProfileHelper.GetPowerSettings(targetPlanGuid, true);
                        }
                        WriteObject(updatedPlan);
                    }
                }
            }
            catch (Exception ex)
            {
                WriteError(new ErrorRecord(ex, "ImportPowerSettingsError", ErrorCategory.NotSpecified, null));
            }
        }

        /// <summary>
        /// Imports a power plan from a JSON file.
        /// </summary>
        /// <param name="path">The path to the input file.</param>
        /// <returns>The imported power plan.</returns>
        private PowerPlan ImportFromJson(string path)
        {
            LoggingHelper.LogVerbose(this, $"Reading JSON file: {path}");
            string json = File.ReadAllText(path);

            LoggingHelper.LogVerbose(this, $"Deserializing JSON data");
            PowerPlan plan = JsonConvert.DeserializeObject<PowerPlan>(json);

            if (plan != null && plan.Settings != null)
            {
                LoggingHelper.LogVerbose(this, $"Successfully deserialized plan '{plan.Name}' with {plan.Settings.Count} settings");
            }
            else
            {
                LoggingHelper.LogVerbose(this, $"JSON deserialization resulted in null or empty plan");
            }

            return plan;
        }

        /// <summary>
        /// Imports a power plan from a CSV file.
        /// </summary>
        /// <param name="path">The path to the input file.</param>
        /// <returns>The imported power plan.</returns>
        private PowerPlan ImportFromCsv(string path)
        {
            LoggingHelper.LogVerbose(this, $"Reading CSV file: {path}");
            string[] lines = File.ReadAllLines(path);
            LoggingHelper.LogVerbose(this, $"Read {lines.Length} lines from CSV file");

            PowerPlan plan = new PowerPlan();
            plan.Settings = new List<PowerSetting>();

            // Extract plan information from header comments if available
            LoggingHelper.LogVerbose(this, $"Parsing CSV header for plan metadata");
            foreach (var line in lines)
            {
                if (line.StartsWith("# Power Plan:"))
                {
                    plan.Name = line.Substring("# Power Plan:".Length).Trim();
                    LoggingHelper.LogVerbose(this, $"Found plan name in header: {plan.Name}");
                }
                else if (line.StartsWith("# GUID:"))
                {
                    string guidStr = line.Substring("# GUID:".Length).Trim();
                    if (Guid.TryParse(guidStr, out Guid guid))
                    {
                        plan.Guid = guid;
                        LoggingHelper.LogVerbose(this, $"Found plan GUID in header: {plan.Guid}");
                    }
                }
                else if (!line.StartsWith("#") && !string.IsNullOrWhiteSpace(line))
                {
                    // Found a non-comment, non-empty line, assume it's the header or data
                    break;
                }
            }

            // Find the header line
            LoggingHelper.LogVerbose(this, $"Searching for CSV column header line");
            int headerIndex = -1;
            for (int i = 0; i < lines.Length; i++)
            {
                if (lines[i].StartsWith("Alias,SettingGuid,SubGroupGuid"))
                {
                    headerIndex = i;
                    LoggingHelper.LogVerbose(this, $"Found header line at index {i}: {lines[i]}");
                    break;
                }
            }

            if (headerIndex == -1)
            {
                string warningMsg = "CSV file does not contain a valid header line. Expected: Alias,SettingGuid,SubGroupGuid,...";
                WriteWarning(warningMsg);
                LoggingHelper.LogVerbose(this, $"WARNING: {warningMsg}");
                return plan;
            }

            // Process data lines
            int dataLineCount = lines.Length - headerIndex - 1;
            LoggingHelper.LogVerbose(this, $"Processing {dataLineCount} data lines");
            int validSettingsCount = 0;

            for (int i = headerIndex + 1; i < lines.Length; i++)
            {
                if (string.IsNullOrWhiteSpace(lines[i]))
                {
                    LoggingHelper.LogVerbose(this, $"Skipping empty line at index {i}", i - headerIndex, dataLineCount);
                    continue;
                }

                try
                {
                    LoggingHelper.LogVerbose(this, $"Parsing line {i}: {lines[i].Substring(0, Math.Min(50, lines[i].Length))}...", i - headerIndex, dataLineCount);

                    // Parse CSV line (handling quoted values with commas)
                    List<string> values = new List<string>();
                    string pattern = @"(?:^|,)(?:""([^""]*)""|([^,]*))";
                    foreach (Match match in Regex.Matches(lines[i], pattern))
                    {
                        string value = match.Groups[1].Success ? match.Groups[1].Value : match.Groups[2].Value;
                        values.Add(value);
                    }

                    LoggingHelper.LogVerbose(this, $"  Extracted {values.Count} values from line");

                    if (values.Count >= 8)
                    {
                        PowerSetting setting = new PowerSetting();
                        setting.Alias = values[0];
                        LoggingHelper.LogVerbose(this, $"  Setting alias: {setting.Alias}");

                        if (Guid.TryParse(values[1], out Guid settingGuid))
                        {
                            setting.SettingGuid = settingGuid;
                            LoggingHelper.LogVerbose(this, $"  Setting GUID: {setting.SettingGuid}");
                        }
                        else
                        {
                            string warningMsg = $"Invalid SettingGuid format in line {i + 1}: {values[1]}";
                            WriteWarning(warningMsg);
                            LoggingHelper.LogVerbose(this, $"  WARNING: {warningMsg}");
                            continue;
                        }

                        if (Guid.TryParse(values[2], out Guid subGroupGuid))
                        {
                            setting.SubGroupGuid = subGroupGuid;
                            LoggingHelper.LogVerbose(this, $"  SubGroup GUID: {setting.SubGroupGuid}");
                        }
                        else
                        {
                            string warningMsg = $"Invalid SubGroupGuid format in line {i + 1}: {values[2]}";
                            WriteWarning(warningMsg);
                            LoggingHelper.LogVerbose(this, $"  WARNING: {warningMsg}");
                            continue;
                        }

                        // Parse OnBattery and PluggedIn values
                        if (!string.IsNullOrEmpty(values[3]))
                        {
                            if (uint.TryParse(values[3], out uint onBattery))
                            {
                                setting.OnBattery = onBattery;
                            }
                            else
                            {
                                setting.OnBattery = values[3];
                            }
                            LoggingHelper.LogVerbose(this, $"  OnBattery value: {setting.OnBattery}");
                        }

                        if (!string.IsNullOrEmpty(values[4]))
                        {
                            if (uint.TryParse(values[4], out uint pluggedIn))
                            {
                                setting.PluggedIn = pluggedIn;
                            }
                            else
                            {
                                setting.PluggedIn = values[4];
                            }
                            LoggingHelper.LogVerbose(this, $"  PluggedIn value: {setting.PluggedIn}");
                        }

                        setting.Units = values[5];
                        setting.Description = values[6];

                        if (bool.TryParse(values[7], out bool isHidden))
                        {
                            setting.IsHidden = isHidden;
                        }

                        plan.Settings.Add(setting);
                        validSettingsCount++;
                        LoggingHelper.LogVerbose(this, $"  Successfully added setting to plan");
                    }
                    else
                    {
                        string warningMsg = $"Skipping line {i + 1}: insufficient values";
                        WriteWarning(warningMsg);
                        LoggingHelper.LogVerbose(this, $"  WARNING: {warningMsg}");
                    }
                }
                catch (Exception ex)
                {
                    string errorMsg = $"Error parsing line {i + 1}: {ex.Message}";
                    WriteWarning(errorMsg);
                    LoggingHelper.LogVerbose(this, $"  ERROR: {errorMsg}");
                }
            }

            LoggingHelper.LogVerbose(this, $"CSV import completed. Successfully parsed {validSettingsCount} settings");
            return plan;
        }

        /// <summary>
        /// Imports a power plan from an XML file.
        /// </summary>
        /// <param name="path">The path to the input file.</param>
        /// <returns>The imported power plan.</returns>
        private PowerPlan ImportFromXml(string path)
        {
            LoggingHelper.LogVerbose(this, $"Creating XML serializer for PowerPlan type");
            XmlSerializer serializer = new XmlSerializer(typeof(PowerPlan));

            LoggingHelper.LogVerbose(this, $"Opening XML file: {path}");
            using (FileStream stream = new FileStream(path, FileMode.Open))
            {
                LoggingHelper.LogVerbose(this, $"Deserializing XML data");
                PowerPlan plan = (PowerPlan)serializer.Deserialize(stream);

                if (plan != null && plan.Settings != null)
                {
                    LoggingHelper.LogVerbose(this, $"Successfully deserialized plan '{plan.Name}' with {plan.Settings.Count} settings");
                }
                else
                {
                    LoggingHelper.LogVerbose(this, $"XML deserialization resulted in null or empty plan");
                }

                return plan;
            }
        }
    }
}
