using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Language;
using PowerPlanTools.Models;

namespace PowerPlanTools.Utils
{
    /// <summary>
    /// Provides argument completers for PowerPlanTools cmdlets
    /// </summary>
    public static class ArgumentCompleters
    {
        #region Power Plan Name Completer

        /// <summary>
        /// Argument completer for power plan names
        /// </summary>
        public class PowerPlanNameCompleter : IArgumentCompleter
        {
            /// <summary>
            /// Completes arguments for power plan names
            /// </summary>
            public IEnumerable<CompletionResult> CompleteArgument(
                string commandName,
                string parameterName,
                string wordToComplete,
                CommandAst commandAst,
                IDictionary fakeBoundParameters)
            {
                // Get all power plans
                var powerPlans = WmiHelper.GetPowerPlans();

                // Filter and sort by name
                return powerPlans
                    .Where(p => string.IsNullOrEmpty(wordToComplete) ||
                                p.Name.IndexOf(wordToComplete, StringComparison.OrdinalIgnoreCase) >= 0)
                    .OrderBy(p => p.Name)
                    .Select(p => new CompletionResult(
                        $"\"{p.Name}\"",
                        p.Name,
                        CompletionResultType.ParameterValue,
                        $"{p.Name} ({(p._IsActive ? "Active" : "Inactive")})"
                    ));
            }
        }

        #endregion

        #region Power Plan GUID Completer

        /// <summary>
        /// Argument completer for power plan GUIDs
        /// </summary>
        public class PowerPlanGuidCompleter : IArgumentCompleter
        {
            /// <summary>
            /// Completes arguments for power plan GUIDs
            /// </summary>
            public IEnumerable<CompletionResult> CompleteArgument(
                string commandName,
                string parameterName,
                string wordToComplete,
                CommandAst commandAst,
                IDictionary fakeBoundParameters)
            {
                // Get all power plans
                var powerPlans = WmiHelper.GetPowerPlans();

                // Filter and sort by GUID
                return powerPlans
                    .Where(p => string.IsNullOrEmpty(wordToComplete) ||
                                p.Guid.ToString().IndexOf(wordToComplete, StringComparison.OrdinalIgnoreCase) >= 0)
                    .OrderBy(p => p.Guid.ToString())
                    .Select(p => new CompletionResult(
                        $"\"{p.Guid}\"",
                        p.Guid.ToString(),
                        CompletionResultType.ParameterValue,
                        $"{p.Guid} ({p.Name})"
                    ));
            }
        }

        #endregion

        #region Power Setting Alias Completer

        /// <summary>
        /// Argument completer for power setting aliases
        /// </summary>
        public class PowerSettingAliasCompleter : IArgumentCompleter
        {
            /// <summary>
            /// Completes arguments for power setting aliases
            /// </summary>
            public IEnumerable<CompletionResult> CompleteArgument(
                string commandName,
                string parameterName,
                string wordToComplete,
                CommandAst commandAst,
                IDictionary fakeBoundParameters)
            {
                // Get the power plan
                Guid planGuid = Guid.Empty;

                // Try to get the plan GUID from parameters
                if (fakeBoundParameters.Contains("PlanGuid") && fakeBoundParameters["PlanGuid"] is Guid guid)
                {
                    planGuid = guid;
                }
                else if (fakeBoundParameters.Contains("PlanName") && fakeBoundParameters["PlanName"] is string planName)
                {
                    var powerPlans = WmiHelper.GetPowerPlans();
                    var plan = powerPlans.Find(p => p.Name.Equals(planName, StringComparison.OrdinalIgnoreCase));
                    if (plan != null)
                    {
                        planGuid = plan.Guid;
                    }
                }

                // If we couldn't get a plan GUID, use the active plan
                if (planGuid == Guid.Empty)
                {
                    planGuid = WmiHelper.GetActivePowerPlanGuid();
                }

                // Get all power settings for the plan
                var settings = WmiHelper.GetPowerSettings(planGuid, true);

                // Filter and sort by alias
                return settings
                    .Where(s => string.IsNullOrEmpty(wordToComplete) ||
                                s.Alias.IndexOf(wordToComplete, StringComparison.OrdinalIgnoreCase) >= 0)
                    .OrderBy(s => s.Alias)
                    .Select(s => new CompletionResult(
                        $"\"{s.Alias}\"",
                        s.Alias,
                        CompletionResultType.ParameterValue,
                        $"{s.Alias} ({s.Description})"
                    ));
            }
        }

        #endregion

        #region Power Setting GUID Completer

        /// <summary>
        /// Argument completer for power setting GUIDs
        /// </summary>
        public class PowerSettingGuidCompleter : IArgumentCompleter
        {
            /// <summary>
            /// Completes arguments for power setting GUIDs
            /// </summary>
            public IEnumerable<CompletionResult> CompleteArgument(
                string commandName,
                string parameterName,
                string wordToComplete,
                CommandAst commandAst,
                IDictionary fakeBoundParameters)
            {
                // Get the power plan
                Guid planGuid = Guid.Empty;

                // Try to get the plan GUID from parameters
                if (fakeBoundParameters.Contains("PlanGuid") && fakeBoundParameters["PlanGuid"] is Guid guid)
                {
                    planGuid = guid;
                }
                else if (fakeBoundParameters.Contains("PlanName") && fakeBoundParameters["PlanName"] is string planName)
                {
                    var powerPlans = WmiHelper.GetPowerPlans();
                    var plan = powerPlans.Find(p => p.Name.Equals(planName, StringComparison.OrdinalIgnoreCase));
                    if (plan != null)
                    {
                        planGuid = plan.Guid;
                    }
                }

                // If we couldn't get a plan GUID, use the active plan
                if (planGuid == Guid.Empty)
                {
                    planGuid = WmiHelper.GetActivePowerPlanGuid();
                }

                // Get all power settings for the plan
                var settings = WmiHelper.GetPowerSettings(planGuid, true);

                // Filter and sort by GUID
                return settings
                    .Where(s => string.IsNullOrEmpty(wordToComplete) ||
                                s.SettingGuid.ToString().IndexOf(wordToComplete, StringComparison.OrdinalIgnoreCase) >= 0)
                    .OrderBy(s => s.SettingGuid.ToString())
                    .Select(s => new CompletionResult(
                        $"\"{s.SettingGuid}\"",
                        s.SettingGuid.ToString(),
                        CompletionResultType.ParameterValue,
                        $"{s.SettingGuid} ({s.Alias})"
                    ));
            }
        }

        #endregion

        #region SubGroup GUID Completer

        /// <summary>
        /// Argument completer for subgroup GUIDs
        /// </summary>
        public class SubGroupGuidCompleter : IArgumentCompleter
        {
            /// <summary>
            /// Completes arguments for subgroup GUIDs
            /// </summary>
            public IEnumerable<CompletionResult> CompleteArgument(
                string commandName,
                string parameterName,
                string wordToComplete,
                CommandAst commandAst,
                IDictionary fakeBoundParameters)
            {
                // Get the power plan
                Guid planGuid = Guid.Empty;

                // Try to get the plan GUID from parameters
                if (fakeBoundParameters.Contains("PlanGuid") && fakeBoundParameters["PlanGuid"] is Guid guid)
                {
                    planGuid = guid;
                }
                else if (fakeBoundParameters.Contains("PlanName") && fakeBoundParameters["PlanName"] is string planName)
                {
                    var powerPlans = WmiHelper.GetPowerPlans();
                    var plan = powerPlans.Find(p => p.Name.Equals(planName, StringComparison.OrdinalIgnoreCase));
                    if (plan != null)
                    {
                        planGuid = plan.Guid;
                    }
                }

                // If we couldn't get a plan GUID, use the active plan
                if (planGuid == Guid.Empty)
                {
                    planGuid = WmiHelper.GetActivePowerPlanGuid();
                }

                // Get all power settings for the plan
                var settings = WmiHelper.GetPowerSettings(planGuid, true);

                // Get unique subgroup GUIDs
                var subGroups = settings
                    .Select(s => s.SubGroupGuid)
                    .Distinct()
                    .ToList();

                // Filter and sort by GUID
                return subGroups
                    .Where(g => string.IsNullOrEmpty(wordToComplete) ||
                                g.ToString().IndexOf(wordToComplete, StringComparison.OrdinalIgnoreCase) >= 0)
                    .OrderBy(g => g.ToString())
                    .Select(g => new CompletionResult(
                        $"\"{g}\"",
                        g.ToString(),
                        CompletionResultType.ParameterValue,
                        g.ToString()
                    ));
            }
        }

        #endregion

        #region Known Power Setting GUID to Alias Mapping

        /// <summary>
        /// Known power setting GUID to alias mapping
        /// </summary>
        public static readonly Dictionary<Guid, string> KnownPowerSettings = new Dictionary<Guid, string>
        {
            // Power Plan GUIDs (for reference)
            // Balanced: 381b4222-f694-41f0-9685-ff5bb260df2e
            // High Performance: 8c5e7fda-e8bf-4a96-9a85-a6e23a8c635c
            // Power Saver: a1841308-3541-4fab-bc81-f71556f20b4a
            // Ultimate Performance: e9a42b02-d5df-448d-aa00-03f14749eb61

            // Power Scheme Subgroups (for reference)
            // SUB_NONE: fea3413e-7e05-4911-9a71-700331f1c294
            // SUB_BATTERY: e73a048d-bf27-4f12-9731-8b2076e8891f
            // SUB_DISK: 0012ee47-9041-4b5d-9b77-535fba8b1442
            // SUB_DISPLAY: 7516b95f-f776-4464-8c53-06167f40cc99
            // SUB_SLEEP: 238c9fa8-0aad-41ed-83f4-97be242c8f20
            // SUB_BUTTONS: 4f971e89-eebd-4455-a8de-9e59040e7347
            // SUB_PCIEXPRESS: 501a4d13-42af-4429-9fd1-a8218c268e20
            // SUB_PROCESSOR: 54533251-82be-4824-96c1-47b60b740d00
            // SUB_MULTIMEDIA: 9596fb26-9850-41fd-ac3e-f7c3c00afd4b
            // SUB_INTERNET: b2decf55-4bd2-45f3-87f5-ffff6cf30c75
            // SUB_GRAPHICS: 5fb4938d-1ee8-4b0f-9a3c-5036b0ab995c
            // SUB_WIRELESS: 19cbb8fa-5279-450e-9fac-8a3d5fedd0c1
            // SUB_SYSTEM: 5d3e9a59-e9d5-4b00-a6bd-ff34ff516548

            // Display settings
            { new Guid("7516b95f-f776-4464-8c53-06167f40cc99"), "Turn off display after" },
            { new Guid("3c0bc021-c8a8-4e07-a973-6b14cbcb2b7e"), "Display brightness" },
            { new Guid("aded5e82-b909-4619-9949-f5d71dac0bcb"), "Dimmed display brightness" },
            { new Guid("f1fbfde2-a960-4165-9f88-50667911ce96"), "Enable adaptive brightness" },
            { new Guid("fbd9aa66-9553-4097-ba44-ed6e9d65eab8"), "Console lock display off timeout" },
            { new Guid("82dbcf2d-cd67-40c5-bfdc-9f1a5ccd4663"), "Adaptive display timeout" },
            { new Guid("90959d22-d6a1-49b9-af93-bce885ad335b"), "Allow display required policy" },
            { new Guid("6fe69556-704a-47a0-8f24-c28d936fda47"), "Dim display after" },
            { new Guid("17aaa29b-8b43-4b94-aafe-35f64daaf1ee"), "Minimum display brightness" },

            // Sleep settings
            { new Guid("29f6c1db-86da-48c5-9fdb-f2b67b1f44da"), "Sleep after" },
            { new Guid("94ac6d29-73ce-41a6-809f-6363ba21b47e"), "Hibernate after" },
            { new Guid("bd3b718a-0680-4d9d-8ab2-e1d2b4ac806d"), "Allow hybrid sleep" },
            { new Guid("9d7815a6-7ee4-497e-8888-515a05f02364"), "Allow wake timers" },
            { new Guid("d4c1d4c8-d5cc-43d3-b83e-fc51215cb04d"), "System unattended sleep timeout" },
            { new Guid("7bc4a2f9-d8fc-4469-b07b-33eb785aaca0"), "Standby reserve time" },
            { new Guid("f23a5d61-7156-45e8-a157-30ab701c6a2a"), "Standby reset percentage" },
            { new Guid("25dfa149-5dd1-4736-b5ab-e8a37b5b8187"), "Allow standby states" },
            { new Guid("abfc2519-3608-4c2a-94ea-171b0ed546ab"), "Allow away mode policy" },
            { new Guid("238c9fa8-0aad-41ed-83f4-97be242c8f20"), "Sleep button action" },
            { new Guid("99ff10e7-23b1-4c07-a9d1-5c3206d741b4"), "Allow sleep with remote opens" },

            // Power buttons and lid
            { new Guid("a7066653-8d6c-40a8-910e-a1f54b84c7e5"), "Power button action" },
            { new Guid("dfee5f4d-0353-4a8c-8d0d-fb66c56a83d9"), "Sleep button action" },
            { new Guid("5ca83367-6e45-459f-a27b-476b1d01c936"), "Lid close action" },
            { new Guid("4f971e89-eebd-4455-a8de-9e59040e7347"), "Start menu power button" },
            { new Guid("96996bc0-ad50-47ec-923b-6f41874dd9eb"), "Lid open action" },

            // Battery settings
            { new Guid("e69653ca-cf7f-4f05-aa73-cb833fa90ad4"), "Critical battery action" },
            { new Guid("637ea02f-bbcb-4015-8e2c-a1c7b9c0b546"), "Critical battery level" },
            { new Guid("d8742dcb-3e6a-4b3c-b3fe-374623cdcf06"), "Low battery level" },
            { new Guid("8183ba9a-e910-48da-8769-14ae6dc1170a"), "Low battery notification" },
            { new Guid("bcded951-187b-4d05-bccc-f7e51960c258"), "Low battery action" },
            { new Guid("9a66d8d7-4ff7-4ef9-b5a2-5a326ca2a469"), "Reserve battery level" },
            { new Guid("f3c5027d-cd16-4930-aa6b-90db844a8f00"), "Battery threshold" },

            // Hard disk settings
            { new Guid("6738e2c4-e8a5-4a42-b16a-e040e769756e"), "Turn off hard disk after" },
            { new Guid("80e3c60e-bb94-4ad8-bbe0-0d3195efc663"), "Hard disk burst ignore time" },
            { new Guid("0b2d69d7-a2a1-449c-9680-f91c70521c60"), "AHCI Link Power Management - HIPM/DIPM" },
            { new Guid("dab60367-53fe-4fbc-825e-521d069d2456"), "Secondary NVMe Idle Timeout" },
            { new Guid("d639518a-e56d-4345-8af2-b9f32fb26109"), "NVME NOPPME" },

            // Wireless adapter settings
            { new Guid("12bbebe6-58d6-4636-95bb-3217ef867c1a"), "Wireless adapter power saving mode" },
            { new Guid("40fdb1df-4f34-44d1-a201-f7b9a1038e24"), "WLAN auto shutdown threshold" },
            { new Guid("c42b79aa-aa3a-484b-a98f-2cf32aa90a28"), "802.11 radio power policy" },
            { new Guid("104d1e6d-7c65-4a8e-b6f4-4f5c0e6a0c43"), "Bluetooth radio power policy" },
            { new Guid("19cbb8fa-5279-450e-9fac-8a3d5fedd0c1"), "Wireless power saving mode" },

            // USB settings
            { new Guid("2a737441-1930-4402-8d77-b2bebba308a3"), "USB selective suspend setting" },
            { new Guid("48e6b7a6-50f5-4782-a5d4-53bb8f07e226"), "USB 3 Link Power Management" },
            { new Guid("d4e98f31-5ffe-4ce1-be31-1b38b384c009"), "USB Hub Selective Suspend Timeout" },

            // PCI Express settings
            { new Guid("ee12f906-d277-404b-b6da-e5fa1a576df5"), "PCI Express power management" },
            { new Guid("501a4d13-42af-4429-9fd1-a8218c268e20"), "Link State Power Management" },
            { new Guid("c763b4ec-0e50-4b6b-9bed-2b92a6ee884e"), "Maximum Power Level" },

            // Processor settings
            { new Guid("54533251-82be-4824-96c1-47b60b740d00"), "Minimum processor state" },
            { new Guid("bc5038f7-23e0-4960-96da-33abaf5935ec"), "Maximum processor state" },
            { new Guid("893dee8e-2bef-41e0-89c6-b55d0929964c"), "Processor performance boost mode" },
            { new Guid("94d3a615-a899-4ac5-ae2b-e4d8f634367f"), "System cooling policy" },
            { new Guid("06cadf0e-64ed-448a-8927-ce7bf90eb35d"), "Processor performance core parking min cores" },
            { new Guid("36687f9e-e3a5-4dbf-b1dc-15eb381c6863"), "Processor energy performance preference policy" },
            { new Guid("40fbefc7-2e9d-4d25-a185-0cfd8574bac6"), "Processor performance core parking max cores" },
            { new Guid("447235c7-6a8d-4cc0-8e24-9eaf70b96e2b"), "Processor idle threshold scaling" },
            { new Guid("4bdaf4e9-d103-46d7-a5f0-6280121616ef"), "Processor idle state maximum" },
            { new Guid("5b33697b-e89d-4d38-aa46-9e7dfb7cd2f9"), "Processor performance time check interval" },
            { new Guid("5d76a2ca-e8c0-402f-a133-2158492d58ad"), "Processor idle disable" },
            { new Guid("68dd2f27-a4ce-4e11-8487-3794e4135dfa"), "Processor performance history count" },
            { new Guid("7d24baa7-0b84-480f-840c-1b0743c00f5f"), "Processor performance decrease policy" },
            { new Guid("8baa4a8a-14c6-4451-8e8b-14bdbd197537"), "Processor performance autonomous mode" },
            { new Guid("8809c2d8-b155-42d4-bcda-0d345651b1db"), "Processor performance increase policy" },
            { new Guid("943c8cb6-6f93-4227-ad87-e9a3feec08d1"), "Processor performance core parking increase time" },
            { new Guid("984cf492-3bed-4488-a8f9-4286c97bf5aa"), "Processor performance core parking parked performance state" },
            { new Guid("9ac18e92-aa3c-4e27-b307-01ae37307129"), "Processor performance core parking decrease policy" },
            { new Guid("c7be0679-2817-4d69-9d02-519a537ed0c6"), "Processor performance core parking increase policy" },
            { new Guid("dfd10d17-d5eb-45dd-877a-9a34ddd15c82"), "Processor performance decrease threshold" },
            { new Guid("df142941-20f3-4edf-9a4a-9c83442d8c85"), "Processor performance core parking decrease time" },
            { new Guid("ea062031-0e34-4ff1-9b6d-eb1059334028"), "Processor performance increase threshold" },
            { new Guid("465e1f50-b610-473a-ab58-00d1077dc418"), "Heterogeneous policy in effect" },
            { new Guid("1facfc65-a930-4bc5-9f38-504ec097bbc0"), "Heterogeneous short running thread scheduling policy" },
            { new Guid("7f2f5cfa-f10c-4823-b5e1-e93ae85f46b5"), "Heterogeneous thread scheduling policy" },
            { new Guid("b000397d-9b0b-483d-98c9-692a6060cfbf"), "Heterogeneous thread schedule policy strict" },

            // Multimedia settings
            { new Guid("03680956-93bc-4294-bba6-4e0f09bb717f"), "When sharing media" },
            { new Guid("34c7b99f-9a6d-4b3c-8dc7-b6693b78cef4"), "When playing video" },
            { new Guid("9596fb26-9850-41fd-ac3e-f7c3c00afd4b"), "Video playback quality bias" },

            // Graphics settings
            { new Guid("5fb4938d-1ee8-4b0f-9a3c-5036b0ab995c"), "GPU preference policy" },
            { new Guid("44f3beca-a7c0-460e-9df2-bb8b99e0cba6"), "Graphics power policy" },
            { new Guid("3619c3f2-afb2-4afc-b0e9-e7fef372de36"), "Adaptive V-Sync" },
            { new Guid("cee4e30b-1bc1-44b8-a5e0-5d5c7aaa55e2"), "Display refresh rate policy" },

            // Network settings
            { new Guid("b2decf55-4bd2-45f3-87f5-ffff6cf30c75"), "Internet Explorer JavaScript Timer Frequency" },
            { new Guid("e73a048d-bf27-4f12-9731-8b2076e8891f"), "Energy Efficient Ethernet" },

            // System settings
            { new Guid("5d3e9a59-e9d5-4b00-a6bd-ff34ff516548"), "Allow Throttle States" },
            { new Guid("0e796bdb-100d-47d6-a2d5-f7d2daa51f51"), "Device idle policy" },
            { new Guid("245d8541-3943-4422-b025-13a784f679b7"), "Power Throttling" },
            { new Guid("2bfc24f9-5ea2-4801-8213-3dbae01aa39d"), "Interrupt steering mode" },
            { new Guid("4b92d758-5a24-4851-a470-815d78aee119"), "Latency Sensitivity Hint" },
            { new Guid("7648efa3-dd9c-4e3e-b566-50f929386280"), "Platform Specific Run Time PM Control" },
            { new Guid("8619b916-e004-4dd8-9b66-dae86f806698"), "Processor Idle Resiliency Timer Resolution" },
            { new Guid("c36f0eb4-2988-4a70-8eee-0884fc2c2433"), "Deep Sleep Enabled" },
            { new Guid("d502f7ee-1dc7-4efd-a55d-f04b6f5c0545"), "Processor Idle Resiliency" },
            { new Guid("e276e160-7cb0-43c6-b20b-73f5dce39954"), "Processor Idle Time Check" },
            { new Guid("e8a1e00e-5c30-430e-a7ae-0992f79d9de8"), "Allow Away Mode" },
            { new Guid("fea3413e-7e05-4911-9a71-700331f1c294"), "Execution Required power request timeout" },

            // Advanced settings
            { new Guid("0853a681-27c8-4100-a2fd-82013e970683"), "Hub Selective Suspend Timeout" },
            { new Guid("0b2d69d7-a2a1-449c-9680-f91c70521c60"), "AHCI Link Power Management - HIPM/DIPM" },
            { new Guid("0e796bdb-100d-47d6-a2d5-f7d2daa51f51"), "Device idle policy" },
            { new Guid("10778347-1370-4ee0-8bbd-33bdacaade49"), "Non-sensor Input Presence Timeout" },
            { new Guid("19cbb8fa-5279-450e-9fac-8a3d5fedd0c1"), "Wireless Adapter Settings" },
            { new Guid("1a34bdc3-7e6b-442e-a9d0-64b6ef378e84"), "Require a password on wakeup" },
            { new Guid("1facfc65-a930-4bc5-9f38-504ec097bbc0"), "Heterogeneous short running thread scheduling policy" },
            { new Guid("2a737441-1930-4402-8d77-b2bebba308a3"), "USB selective suspend setting" },
            { new Guid("2bfc24f9-5ea2-4801-8213-3dbae01aa39d"), "Interrupt steering mode" },
            { new Guid("2e601130-5351-4d9d-8e04-a110a0c8a627"), "A/C Lid Open Action" },
            { new Guid("3619c3f2-afb2-4afc-b0e9-e7fef372de36"), "Adaptive V-Sync" },
            { new Guid("3c0bc021-c8a8-4e07-a973-6b14cbcb2b7e"), "Display brightness" },
            { new Guid("3c796d24-3d6d-483c-a1f7-d2f1d7bfbba2"), "Enable forced button shutdown" },
            { new Guid("44f3beca-a7c0-460e-9df2-bb8b99e0cba6"), "Graphics power policy" },
            { new Guid("45bcc044-d885-43e2-8605-ee0ec6e96b59"), "Allow RTC wake from S4" },
            { new Guid("465e1f50-b610-473a-ab58-00d1077dc418"), "Heterogeneous policy in effect" },
            { new Guid("48672f38-7a9a-4bb2-8bf8-3d85be19de4e"), "D3 cold enabled" },
            { new Guid("48e6b7a6-50f5-4782-a5d4-53bb8f07e226"), "USB 3 Link Power Management" },
            { new Guid("4b92d758-5a24-4851-a470-815d78aee119"), "Latency Sensitivity Hint" },
            { new Guid("4e4450b3-6179-4e91-b8f1-5bb9938f81a1"), "Low Power S0 Idle Capability" },
            { new Guid("501a4d13-42af-4429-9fd1-a8218c268e20"), "PCI Express" },
            { new Guid("51dea550-bb38-4bc4-991b-eacf37be5ec8"), "NVMe Power State Transition Latency Tolerance" },
            { new Guid("543e0f88-611b-4eab-bf9d-c9c38790c55e"), "Disconnected Standby Mode" },
            { new Guid("5513da27-cd7a-44f2-8473-679c1b8d128d"), "Processor Duty Cycling" },
            { new Guid("5d3e9a59-e9d5-4b00-a6bd-ff34ff516548"), "Allow Throttle States" },
            { new Guid("5fb4938d-1ee8-4b0f-9a3c-5036b0ab995c"), "GPU preference policy" },
            { new Guid("6c2993b0-8f48-481f-bcc6-00dd2742aa06"), "Secondary NVMe Power State Transition Latency Tolerance" },
            { new Guid("7648efa3-dd9c-4e3e-b566-50f929386280"), "Platform Specific Run Time PM Control" },
            { new Guid("7f2f5cfa-f10c-4823-b5e1-e93ae85f46b5"), "Heterogeneous thread scheduling policy" },
            { new Guid("8619b916-e004-4dd8-9b66-dae86f806698"), "Processor Idle Resiliency Timer Resolution" },
            { new Guid("8619b916-e004-4dd8-9b66-dae86f806698"), "Processor Idle Resiliency Timer Resolution" },
            { new Guid("8ec689a2-7f37-4f21-a117-d5a9a73a9095"), "Lid Open Action" },
            { new Guid("9596fb26-9850-41fd-ac3e-f7c3c00afd4b"), "Multimedia settings" },
            { new Guid("9fe527be-1b70-48da-930d-7bcf17b44990"), "Power button override" },
            { new Guid("a1841308-3541-4fab-bc81-f71556f20b4a"), "Power Saver" },
            { new Guid("a4b195f5-8225-47d8-8012-9d41369786e2"), "Allow Timed Input" },
            { new Guid("a55612aa-f624-42c6-a443-7397d064c04f"), "Intel(R) Graphics Power Plan" },
            { new Guid("aded5e82-b909-4619-9949-f5d71dac0bcb"), "Dimmed display brightness" },
            { new Guid("b000397d-9b0b-483d-98c9-692a6060cfbf"), "Heterogeneous thread schedule policy strict" },
            { new Guid("b2decf55-4bd2-45f3-87f5-ffff6cf30c75"), "Internet Explorer" },
            { new Guid("c36f0eb4-2988-4a70-8eee-0884fc2c2433"), "Deep Sleep Enabled" },
            { new Guid("c4c1ae1c-f18c-4a9d-9825-acf915b96ffe"), "Unattended Mode" },
            { new Guid("cee4e30b-1bc1-44b8-a5e0-5d5c7aaa55e2"), "Display refresh rate policy" },
            { new Guid("d4c1d4c8-d5cc-43d3-b83e-fc51215cb04d"), "System unattended sleep timeout" },
            { new Guid("d4e98f31-5ffe-4ce1-be31-1b38b384c009"), "USB Hub Selective Suspend Timeout" },
            { new Guid("d502f7ee-1dc7-4efd-a55d-f04b6f5c0545"), "Processor Idle Resiliency" },
            { new Guid("d639518a-e56d-4345-8af2-b9f32fb26109"), "NVME NOPPME" },
            { new Guid("dab60367-53fe-4fbc-825e-521d069d2456"), "Secondary NVMe Idle Timeout" },
            { new Guid("e276e160-7cb0-43c6-b20b-73f5dce39954"), "Processor Idle Time Check" },
            { new Guid("e73a048d-bf27-4f12-9731-8b2076e8891f"), "Energy Efficient Ethernet" },
            { new Guid("e8a1e00e-5c30-430e-a7ae-0992f79d9de8"), "Allow Away Mode" },
            { new Guid("f15576e8-98b7-4186-b944-eafa664402d9"), "Power Saving Mode" },
            { new Guid("f1fbfde2-a960-4165-9f88-50667911ce96"), "Enable adaptive brightness" },
            { new Guid("f3c5027d-cd16-4930-aa6b-90db844a8f00"), "Battery threshold" },
            { new Guid("fea3413e-7e05-4911-9a71-700331f1c294"), "Execution Required power request timeout" }
        };

        #endregion

        #region Helper Methods

        /// <summary>
        /// Gets the friendly alias for a power setting GUID
        /// </summary>
        /// <param name="settingGuid">The GUID of the power setting</param>
        /// <returns>The friendly alias if known, or the GUID string if not</returns>
        public static string GetPowerSettingAlias(Guid settingGuid)
        {
            if (KnownPowerSettings.TryGetValue(settingGuid, out string alias))
            {
                return alias;
            }

            return settingGuid.ToString();
        }

        /// <summary>
        /// Gets the GUID for a power setting alias
        /// </summary>
        /// <param name="settingAlias">The alias of the power setting</param>
        /// <returns>The GUID if found, or Guid.Empty if not</returns>
        public static Guid GetPowerSettingGuid(string settingAlias)
        {
            foreach (var kvp in KnownPowerSettings)
            {
                if (kvp.Value.Equals(settingAlias, StringComparison.OrdinalIgnoreCase))
                {
                    return kvp.Key;
                }
            }

            return Guid.Empty;
        }

        #endregion
    }
}
