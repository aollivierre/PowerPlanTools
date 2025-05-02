using System;
using System.IO;
using System.Management.Automation;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using Newtonsoft.Json;
using PowerPlanTools.Models;
using PowerPlanTools.Utils;

namespace PowerPlanTools.Cmdlets
{
    /// <summary>
    /// <para type="synopsis">Exports power settings to a file.</para>
    /// <para type="description">The Export-PowerSettings cmdlet exports power settings for a Windows power plan to a file.</para>
    /// <para type="description">You can specify a plan by name or GUID, and export in JSON, CSV, or XML format.</para>
    /// <example>
    ///     <para>Export power settings to a JSON file</para>
    ///     <code>Export-PowerSettings -PlanName "Balanced" -Path "C:\Reports\BalancedPlan.json" -Format Json</code>
    /// </example>
    /// <example>
    ///     <para>Export power settings to a CSV file</para>
    ///     <code>Export-PowerSettings -PlanName "Balanced" -Path "C:\Reports\BalancedPlan.csv" -Format Csv</code>
    /// </example>
    /// <example>
    ///     <para>Export power settings to an XML file</para>
    ///     <code>Export-PowerSettings -PlanGuid "381b4222-f694-41f0-9685-ff5bb260df2e" -Path "C:\Reports\BalancedPlan.xml" -Format Xml</code>
    /// </example>
    /// </summary>
    [Cmdlet(VerbsData.Export, "PowerSettings", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
    public class ExportPowerSettingsCmdlet : PSCmdlet
    {
        /// <summary>
        /// <para type="description">Gets or sets the name of the power plan.</para>
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipeline = true, ValueFromPipelineByPropertyName = true, ParameterSetName = "ByPlanName")]
        [ValidateNotNullOrEmpty]
        public string PlanName { get; set; }

        /// <summary>
        /// <para type="description">Gets or sets the GUID of the power plan.</para>
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = true, ParameterSetName = "ByPlanGuid")]
        public Guid PlanGuid { get; set; }

        /// <summary>
        /// <para type="description">Gets or sets the path to the output file.</para>
        /// </summary>
        [Parameter(Mandatory = true, Position = 0)]
        [ValidateNotNullOrEmpty]
        public string Path { get; set; }

        /// <summary>
        /// <para type="description">Gets or sets the output format.</para>
        /// </summary>
        [Parameter]
        [ValidateSet("Json", "Csv", "Xml")]
        public string Format { get; set; } = "Json";

        /// <summary>
        /// <para type="description">Gets or sets whether to include hidden settings in the output.</para>
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
                // Get the power plan GUID and name
                Guid planGuid;
                string planName;

                if (ParameterSetName == "ByPlanName")
                {
                    // Find the plan by name
                    var powerPlans = UsePowrProf ? PowerProfileHelper.GetPowerPlans() : WmiHelper.GetPowerPlans();
                    var plan = powerPlans.Find(p => p.Name.Equals(PlanName, StringComparison.OrdinalIgnoreCase));

                    if (plan == null)
                    {
                        WriteError(new ErrorRecord(
                            new ArgumentException($"Power plan '{PlanName}' not found."),
                            "PowerPlanNotFound",
                            ErrorCategory.ObjectNotFound,
                            PlanName));
                        return;
                    }

                    planGuid = plan.Guid;
                    planName = plan.Name;
                }
                else
                {
                    // Use the provided GUID
                    planGuid = PlanGuid;

                    // Get the plan name for display
                    var powerPlans = UsePowrProf ? PowerProfileHelper.GetPowerPlans() : WmiHelper.GetPowerPlans();
                    var plan = powerPlans.Find(p => p.Guid == planGuid);
                    planName = plan?.Name ?? planGuid.ToString();
                }

                // Get power settings
                var settings = UsePowrProf ?
                    PowerProfileHelper.GetPowerSettings(planGuid, IncludeHidden) :
                    WmiHelper.GetPowerSettings(planGuid, IncludeHidden);

                // Create a power plan object with settings
                var powerPlan = new PowerPlan
                {
                    Name = planName,
                    Guid = planGuid,
                    Settings = settings
                };

                // Confirm the action
                if (!ShouldProcess($"Export power settings for plan '{planName}' to '{Path}'", "Export-PowerSettings"))
                {
                    return;
                }

                // Export to the specified format
                switch (Format.ToLowerInvariant())
                {
                    case "json":
                        ExportToJson(powerPlan, Path);
                        break;
                    case "csv":
                        ExportToCsv(powerPlan, Path);
                        break;
                    case "xml":
                        ExportToXml(powerPlan, Path);
                        break;
                    default:
                        WriteError(new ErrorRecord(
                            new ArgumentException($"Unsupported format: {Format}"),
                            "UnsupportedFormat",
                            ErrorCategory.InvalidArgument,
                            Format));
                        return;
                }

                LoggingHelper.LogVerbose(this, $"Power settings for plan '{planName}' exported to '{Path}' in {Format} format. Total settings: {settings.Count}");
            }
            catch (Exception ex)
            {
                WriteError(new ErrorRecord(ex, "ExportPowerSettingsError", ErrorCategory.NotSpecified, null));
            }
        }



        /// <summary>
        /// Ensures that the directory for the specified path exists.
        /// </summary>
        /// <param name="path">The file path.</param>
        private void EnsureDirectoryExists(string path)
        {
            string directory = System.IO.Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                LoggingHelper.LogVerbose(this, $"Creating directory: {directory}");
                Directory.CreateDirectory(directory);
            }
        }

        /// <summary>
        /// Exports a power plan to a JSON file.
        /// </summary>
        /// <param name="powerPlan">The power plan to export.</param>
        /// <param name="path">The path to the output file.</param>
        private void ExportToJson(PowerPlan powerPlan, string path)
        {
            LoggingHelper.LogVerbose(this, $"Starting JSON export for plan '{powerPlan.Name}' ({powerPlan.Guid})");
            EnsureDirectoryExists(path);

            LoggingHelper.LogVerbose(this, $"Serializing {powerPlan.Settings.Count} power settings to JSON");
            string json = JsonConvert.SerializeObject(powerPlan, Newtonsoft.Json.Formatting.Indented);

            LoggingHelper.LogVerbose(this, $"Writing JSON data to file: {path}");
            File.WriteAllText(path, json);

            LoggingHelper.LogVerbose(this, $"JSON export completed successfully");
        }

        /// <summary>
        /// Exports a power plan to a CSV file.
        /// </summary>
        /// <param name="powerPlan">The power plan to export.</param>
        /// <param name="path">The path to the output file.</param>
        private void ExportToCsv(PowerPlan powerPlan, string path)
        {
            LoggingHelper.LogVerbose(this, $"Starting CSV export for plan '{powerPlan.Name}' ({powerPlan.Guid})");
            EnsureDirectoryExists(path);
            StringBuilder csv = new StringBuilder();

            // Add header with plan information
            LoggingHelper.LogVerbose(this, $"Adding plan metadata to CSV header");
            csv.AppendLine($"# Power Plan: {powerPlan.Name}");
            csv.AppendLine($"# GUID: {powerPlan.Guid}");
            csv.AppendLine($"# Export Date: {DateTime.Now}");
            csv.AppendLine();

            // Add settings header
            csv.AppendLine("Alias,SettingGuid,SubGroupGuid,OnBattery,PluggedIn,Units,Description,IsHidden");

            // Add settings
            int settingCount = powerPlan.Settings.Count;
            LoggingHelper.LogVerbose(this, $"Processing {settingCount} power settings for CSV export");

            for (int i = 0; i < settingCount; i++)
            {
                var setting = powerPlan.Settings[i];
                LoggingHelper.LogVerbose(this, $"Processing setting: {setting.Alias}", i + 1, settingCount);
                csv.AppendLine($"\"{setting.Alias}\",\"{setting.SettingGuid}\",\"{setting.SubGroupGuid}\",\"{setting.OnBattery}\",\"{setting.PluggedIn}\",\"{setting.Units}\",\"{setting.Description?.Replace("\"", "\"\"")}\",\"{setting.IsHidden}\"");
            }

            LoggingHelper.LogVerbose(this, $"Writing CSV data to file: {path}");
            File.WriteAllText(path, csv.ToString());

            LoggingHelper.LogVerbose(this, $"CSV export completed successfully");
        }

        /// <summary>
        /// Exports a power plan to an XML file.
        /// </summary>
        /// <param name="powerPlan">The power plan to export.</param>
        /// <param name="path">The path to the output file.</param>
        private void ExportToXml(PowerPlan powerPlan, string path)
        {
            LoggingHelper.LogVerbose(this, $"Starting XML export for plan '{powerPlan.Name}' ({powerPlan.Guid})");
            EnsureDirectoryExists(path);

            LoggingHelper.LogVerbose(this, $"Creating XML serializer for {powerPlan.Settings.Count} power settings");
            XmlSerializer serializer = new XmlSerializer(typeof(PowerPlan));
            XmlWriterSettings settings = new XmlWriterSettings
            {
                Indent = true,
                IndentChars = "  "
            };

            LoggingHelper.LogVerbose(this, $"Writing XML data to file: {path}");
            using (XmlWriter writer = XmlWriter.Create(path, settings))
            {
                serializer.Serialize(writer, powerPlan);
            }

            LoggingHelper.LogVerbose(this, $"XML export completed successfully");
        }
    }
}
