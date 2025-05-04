using System;
using System.Collections;
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
                        $"{p.Name} ({(p.IsActive ? "Active" : "Inactive")})"
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
                const string PLAN_GUID_PARAM = "PlanGuid";
                const string PLAN_NAME_PARAM = "PlanName";
                Guid planGuid = Guid.Empty;

                // Try to get the plan GUID from parameters
                if (fakeBoundParameters.Contains(PLAN_GUID_PARAM) && fakeBoundParameters[PLAN_GUID_PARAM] is Guid guid)
                {
                    planGuid = guid;
                }
                else if (fakeBoundParameters.Contains(PLAN_NAME_PARAM) && fakeBoundParameters[PLAN_NAME_PARAM] is string planName)
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
                const string PLAN_GUID_PARAM = "PlanGuid";
                const string PLAN_NAME_PARAM = "PlanName";
                Guid planGuid = Guid.Empty;

                // Try to get the plan GUID from parameters
                if (fakeBoundParameters.Contains(PLAN_GUID_PARAM) && fakeBoundParameters[PLAN_GUID_PARAM] is Guid guid)
                {
                    planGuid = guid;
                }
                else if (fakeBoundParameters.Contains(PLAN_NAME_PARAM) && fakeBoundParameters[PLAN_NAME_PARAM] is string planName)
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
                const string PLAN_GUID_PARAM = "PlanGuid";
                const string PLAN_NAME_PARAM = "PlanName";
                Guid planGuid = Guid.Empty;

                // Try to get the plan GUID from parameters
                if (fakeBoundParameters.Contains(PLAN_GUID_PARAM) && fakeBoundParameters[PLAN_GUID_PARAM] is Guid guid)
                {
                    planGuid = guid;
                }
                else if (fakeBoundParameters.Contains(PLAN_NAME_PARAM) && fakeBoundParameters[PLAN_NAME_PARAM] is string planName)
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
        private static readonly Dictionary<Guid, string> _knownPowerSettings = new Dictionary<Guid, string>
        {
                // Sleep settings
                { new Guid("29f6c1db-86da-48c5-9fdb-f2b67b1f44da"), "Sleep after" },
                { new Guid("94ac6d29-73ce-41a6-809f-6363ba21b47e"), "Hibernate after" },
                { new Guid("bd3b718a-0680-4d9d-8ab2-e1d2b4ac806d"), "Allow hybrid sleep" },
                { new Guid("9d7815a6-7ee4-497e-8888-515a05f02364"), "Allow wake timers" },
                { new Guid("d4c1d4c8-d5cc-43d3-b83e-fc51215cb04d"), "System unattended sleep timeout" },
                { new Guid("7bc4a2f9-d8fc-4469-b07b-33eb785aaca0"), "Standby reserve time" },

                // Power buttons and lid
                { new Guid("a7066653-8d6c-40a8-910e-a1f54b84c7e5"), "Power button action" },
                { new Guid("5ca83367-6e45-459f-a27b-476b1d01c936"), "Lid close action" },

                // Hard disk settings
                { new Guid("6738e2c4-e8a5-4a42-b16a-e040e769756e"), "Turn off hard disk after" },
                { new Guid("80e3c60e-bb94-4ad8-bbe0-0d3195efc663"), "Hard disk burst ignore time" },
                { new Guid("0b2d69d7-a2a1-449c-9680-f91c70521c60"), "AHCI Link Power Management - HIPM/DIPM" },

                // Processor settings
                { new Guid("bc5038f7-23e0-4960-96da-33abaf5935ec"), "Maximum processor state" },
                { new Guid("94d3a615-a899-4ac5-ae2b-e4d8f634367f"), "System cooling policy" },
                { new Guid("dfd10d17-d5eb-45dd-877a-9a34ddd15c82"), "Processor performance decrease threshold" },

                // Additional settings
                { new Guid("51dea550-bb38-4bc4-991b-eacf37be5ec8"), "NVMe Power State Transition Latency Tolerance" },
                { new Guid("d3d55efd-c1ff-424e-9dc3-441be7833010"), "NVMe APST Settings" },
                { new Guid("5fb4938d-1ee8-4b0f-9a3c-5036b0ab995c"), "GPU preference policy" },
                { new Guid("12bbebe6-58d6-4636-95bb-3217ef867c1a"), "USB Selective Suspend Setting" },
                { new Guid("501a4d13-42af-4429-9fd1-a8218c268e20"), "Link State Power Management" },

                // Additional unknown settings
                { new Guid("dbc9e238-6de9-49e3-92cd-8c2b4946b472"), "Modern Standby" },
                { new Guid("fc7372b6-ab2d-43ee-8797-15e9841f2cca"), "Connected Standby" },
                { new Guid("fc95af4d-40e7-4b6d-835a-56d131dbc80e"), "Standby Budget Grace Period" },
                { new Guid("498c044a-201b-4631-a522-5c744ed4e678"), "PCI Express ASPM Policy" },
                { new Guid("3166bc41-7e98-4e03-b34e-ec0f5f2b218e"), "Processor Idle Demote Threshold" },
                { new Guid("d639518a-e56d-4345-8af2-b9f32fb26109"), "NVME NOPPME" },
                { new Guid("dab60367-53fe-4fbc-825e-521d069d2456"), "Secondary NVMe Idle Timeout" },
                { new Guid("1a34bdc3-7e6b-442e-a9d0-64b6ef378e84"), "Require a password on wakeup" },
                { new Guid("25dfa149-5dd1-4736-b5ab-e8a37b5b8187"), "Allow standby states" },
                { new Guid("a4b195f5-8225-47d8-8012-9d41369786e2"), "Allow Timed Input" },
                { new Guid("abfc2519-3608-4c2a-94ea-171b0ed546ab"), "Allow away mode policy" },
                { new Guid("0853a681-27c8-4100-a2fd-82013e970683"), "Hub Selective Suspend Timeout" },
                { new Guid("48e6b7a6-50f5-4782-a5d4-53bb8f07e226"), "USB 3 Link Power Management" },
                { new Guid("d4e98f31-5ffe-4ce1-be31-1b38b384c009"), "USB Hub Selective Suspend Timeout" },
                { new Guid("c36f0eb4-2988-4a70-8eee-0884fc2c2433"), "Deep Sleep Enabled" },
                { new Guid("c42b79aa-aa3a-484b-a98f-2cf32aa90a28"), "802.11 radio power policy" },
                { new Guid("d502f7ee-1dc7-4efd-a55d-f04b6f5c0545"), "Processor Idle Resiliency" },
                { new Guid("2bfc24f9-5ea2-4801-8213-3dbae01aa39d"), "Interrupt steering mode" },
                { new Guid("73cde64d-d720-4bb2-a860-c755afe77ef2"), "Power Button Action (tablet)" },
                { new Guid("d6ba4903-386f-4c2c-8adb-5c21b3328d25"), "Power Button Action (lid)" },
                { new Guid("7648efa3-dd9c-4e3e-b566-50f929386280"), "Platform Specific Run Time PM Control" },
                { new Guid("833a6b62-dfa4-46d1-82f8-e09e34d029d6"), "Adaptive Brightness" },
                { new Guid("96996bc0-ad50-47ec-923b-6f41874dd9eb"), "Lid open action" },
                { new Guid("99ff10e7-23b1-4c07-a9d1-5c3206d741b4"), "Allow sleep with remote opens" },
                { new Guid("06cadf0e-64ed-448a-8927-ce7bf90eb35d"), "Processor performance core parking min cores" },
                { new Guid("06cadf0e-64ed-448a-8927-ce7bf90eb35e"), "Processor performance core parking min cores for Processor Power Efficiency Class 1" },
                { new Guid("0cc5b647-c1df-4637-891a-dec35c318583"), "Allow Away Mode with Media Sharing" },
                { new Guid("0cc5b647-c1df-4637-891a-dec35c318584"), "Allow Away Mode when playing media" },
                { new Guid("12a0ab44-fe28-4fa9-b3bd-4b64f44960a6"), "System cooling policy for Processor Power Efficiency Class 0" },
                { new Guid("12a0ab44-fe28-4fa9-b3bd-4b64f44960a7"), "System cooling policy for Processor Power Efficiency Class 1" },
                { new Guid("1a98ad09-af22-42ca-8e61-f0a5802c270a"), "Processor Performance Increase Threshold for Processor Power Efficiency Class 1" },
                { new Guid("1facfc65-a930-4bc5-9f38-504ec097bbc0"), "Heterogeneous short running thread scheduling policy" },
                { new Guid("2430ab6f-a520-44a2-9601-f7f23b5134b1"), "Processor Performance Decrease Threshold for Processor Power Efficiency Class 1" },
                { new Guid("2ddd5a84-5a71-437e-912a-db0b8c788732"), "Processor Performance Increase Policy for Processor Power Efficiency Class 1" },
                { new Guid("36687f9e-e3a5-4dbf-b1dc-15eb381c6863"), "Processor energy performance preference policy" },
                { new Guid("36687f9e-e3a5-4dbf-b1dc-15eb381c6864"), "Processor energy performance preference policy for Processor Power Efficiency Class 1" },
                { new Guid("3b04d4fd-1cc7-4f23-ab1c-d1337819c4bb"), "Processor Performance Decrease Policy for Processor Power Efficiency Class 1" },
                { new Guid("4009efa7-e72d-4cba-9edf-91084ea8cbc3"), "Processor Idle Promote Threshold" },
                { new Guid("40fbefc7-2e9d-4d25-a185-0cfd8574bac6"), "Processor performance core parking max cores" },
                { new Guid("40fbefc7-2e9d-4d25-a185-0cfd8574bac7"), "Processor performance core parking max cores for Processor Power Efficiency Class 1" },
                { new Guid("43f278bc-0f8a-46d0-8b31-9a23e615d713"), "Processor Performance Increase Time for Processor Power Efficiency Class 1" },
                { new Guid("447235c7-6a8d-4cc0-8e24-9eaf70b96e2b"), "Processor idle threshold scaling" },
                { new Guid("447235c7-6a8d-4cc0-8e24-9eaf70b96e2c"), "Processor idle threshold scaling for Processor Power Efficiency Class 1" },
                { new Guid("45bcc044-d885-43e2-8605-ee0ec6e96b59"), "Allow RTC wake from S4" },
                { new Guid("465e1f50-b610-473a-ab58-00d1077dc418"), "Heterogeneous policy in effect" },
                { new Guid("465e1f50-b610-473a-ab58-00d1077dc419"), "Heterogeneous policy in effect for Processor Power Efficiency Class 1" },
                { new Guid("4b70f900-cdd9-4e66-aa26-ae8417f98173"), "Minimum processor state for Processor Power Efficiency Class 0" },
                { new Guid("4b70f900-cdd9-4e66-aa26-ae8417f98174"), "Minimum processor state for Processor Power Efficiency Class 1" },
                { new Guid("4b92d758-5a24-4851-a470-815d78aee119"), "Latency Sensitivity Hint" },
                { new Guid("4bdaf4e9-d103-46d7-a5f0-6280121616ef"), "Processor idle state maximum" },
                { new Guid("4d2b0152-7d5c-498b-88e2-34345392a2c5"), "Processor Performance Increase Time" },
                { new Guid("4e4450b3-6179-4e91-b8f1-5bb9938f81a1"), "Low Power S0 Idle Capability" },
                { new Guid("53824d46-87bd-4739-aa1b-aa793fac36d6"), "Processor Performance Decrease Time for Processor Power Efficiency Class 1" },
                { new Guid("5d76a2ca-e8c0-402f-a133-2158492d58ad"), "Processor idle disable" },
                { new Guid("603fe9ce-8d01-4b48-a968-1d706c28fd5c"), "Maximum processor state for Processor Power Efficiency Class 0" },
                { new Guid("603fe9ce-8d01-4b48-a968-1d706c28fd5d"), "Maximum processor state for Processor Power Efficiency Class 1" },
                { new Guid("60fbe21b-efd9-49f2-b066-8674d8e9f423"), "Processor Performance Core Parking Utility Distribution Threshold for Processor Power Efficiency Class 1" },
                { new Guid("616cdaa5-695e-4545-97ad-97dc2d1bdd88"), "Processor Performance Core Parking Concurrency Headroom Threshold for Processor Power Efficiency Class 0" },
                { new Guid("616cdaa5-695e-4545-97ad-97dc2d1bdd89"), "Processor Performance Core Parking Concurrency Headroom Threshold for Processor Power Efficiency Class 1" },
                { new Guid("619b7505-003b-4e82-b7a6-4dd29c300971"), "Processor Performance Core Parking Concurrency Threshold for Processor Power Efficiency Class 0" },
                { new Guid("619b7505-003b-4e82-b7a6-4dd29c300972"), "Processor Performance Core Parking Concurrency Threshold for Processor Power Efficiency Class 1" },
                { new Guid("64fcee6b-5b1f-45a4-a76a-19b2c36ee290"), "Processor Performance Core Parking Increase Threshold for Processor Power Efficiency Class 1" },
                { new Guid("6788488b-1b90-4d11-8fa7-973e470dff47"), "Processor Performance Core Parking Decrease Threshold for Processor Power Efficiency Class 1" },
                { new Guid("69439b22-221b-4830-bd34-f7bcece24583"), "Processor Performance Core Parking Overutilization Threshold for Processor Power Efficiency Class 1" },
                { new Guid("6c2993b0-8f48-481f-bcc6-00dd2742aa06"), "Secondary NVMe Power State Transition Latency Tolerance" },
                { new Guid("6ff13aeb-7897-4356-9999-dd9930af065f"), "Processor Performance Core Parking Overutilization Weighting for Processor Power Efficiency Class 1" },
                { new Guid("71021b41-c749-4d21-be74-a00f335d582b"), "Processor Performance Core Parking Decrease Threshold" },
                { new Guid("75b0ae3f-bce0-45a7-8c89-c9611c25e100"), "Processor Performance Core Parking Increase Policy for Processor Power Efficiency Class 0" },
                { new Guid("75b0ae3f-bce0-45a7-8c89-c9611c25e101"), "Processor Performance Core Parking Increase Policy for Processor Power Efficiency Class 1" },
                { new Guid("7b224883-b3cc-4d79-819f-8374152cbe7c"), "Processor Performance Core Parking Utility Distribution" },
                { new Guid("7d24baa7-0b84-480f-840c-1b0743c00f5f"), "Processor performance decrease policy" },
                { new Guid("7d24baa7-0b84-480f-840c-1b0743c00f60"), "Processor performance decrease policy for Processor Power Efficiency Class 1" },
                { new Guid("7f2492b6-60b1-45e5-ae55-773f8cd5caec"), "Processor Performance Core Parking Decrease Time for Processor Power Efficiency Class 1" },
                { new Guid("7f2f5cfa-f10c-4823-b5e1-e93ae85f46b5"), "Heterogeneous thread scheduling policy" },
                { new Guid("828423eb-8662-4344-90f7-52bf15870f5a"), "Processor Performance Core Parking Increase Time for Processor Power Efficiency Class 1" },
                { new Guid("893dee8e-2bef-41e0-89c6-b55d0929964c"), "Processor performance boost mode" },
                { new Guid("893dee8e-2bef-41e0-89c6-b55d0929964d"), "Processor performance boost mode for Processor Power Efficiency Class 1" },
                { new Guid("8baa4a8a-14c6-4451-8e8b-14bdbd197537"), "Processor performance autonomous mode" },
                { new Guid("93b8b6dc-0698-4d1c-9ee4-0644e900c85d"), "Processor Performance Core Parking Parked Performance State for Processor Power Efficiency Class 1" },
                { new Guid("943c8cb6-6f93-4227-ad87-e9a3feec08d1"), "Processor performance core parking increase time" },
                { new Guid("97cfac41-2217-47eb-992d-618b1977c907"), "Processor Performance Core Parking Overutilization Threshold" },
                { new Guid("984cf492-3bed-4488-a8f9-4286c97bf5aa"), "Processor performance core parking parked performance state" },
                { new Guid("984cf492-3bed-4488-a8f9-4286c97bf5ab"), "Processor performance core parking parked performance state for Processor Power Efficiency Class 1" },
                { new Guid("9943e905-9a30-4ec1-9b99-44dd3b76f7a2"), "Processor Performance Core Parking Overutilization Weighting" },
                { new Guid("b000397d-9b0b-483d-98c9-692a6060cfbf"), "Heterogeneous thread schedule policy strict" },
                { new Guid("b000397d-9b0b-483d-98c9-692a6060cfc0"), "Heterogeneous thread schedule policy strict for Processor Power Efficiency Class 1" },
                { new Guid("b0deaf6b-59c0-4523-8a45-ca7f40244114"), "Processor Performance Core Parking Affinity History for Processor Power Efficiency Class 1" },
                { new Guid("b28a6829-c5f7-444e-8f61-10e24e85c532"), "Processor Performance Core Parking Utility Distribution for Processor Power Efficiency Class 1" },
                { new Guid("b669a5e9-7b1d-4132-baaa-49190abcfeb6"), "Processor Performance Core Parking Decrease Policy" },
                { new Guid("bae08b81-2d5e-4688-ad6a-13243356654b"), "Processor Performance Core Parking Decrease Policy for Processor Power Efficiency Class 1" },
                { new Guid("bc5038f7-23e0-4960-96da-33abaf5935ed"), "Maximum processor state" },
                { new Guid("be337238-0d82-4146-a960-4f3749d470c7"), "Processor Performance Core Parking Increase Threshold" },
                { new Guid("bf903d33-9d24-49d3-a468-e65e0325046a"), "Processor Performance Core Parking Increase Policy for Processor Power Efficiency Class 1" },
                { new Guid("c4581c31-89ab-4597-8e2b-9c9cab440e6b"), "Processor Performance Core Parking Max Cores" },
                { new Guid("c7be0679-2817-4d69-9d02-519a537ed0c6"), "Processor performance core parking increase policy" },
                { new Guid("cfeda3d0-7697-4566-a922-a9086cd49dfa"), "Processor Performance Core Parking Min Cores for Processor Power Efficiency Class 1" },
                { new Guid("d8edeb9b-95cf-4f95-a73c-b061973693c8"), "Processor Performance Increase Policy for Processor Power Efficiency Class 0" },
                { new Guid("d8edeb9b-95cf-4f95-a73c-b061973693c9"), "Processor Performance Increase Policy for Processor Power Efficiency Class 1" },
                { new Guid("d92998c2-6a48-49ca-85d4-8cceec294570"), "Processor Performance Core Parking Increase Threshold for Processor Power Efficiency Class 1" },
                { new Guid("e0007330-f589-42ed-a401-5ddb10e785d3"), "Processor Performance Core Parking Decrease Threshold for Processor Power Efficiency Class 1" },
                { new Guid("ea062031-0e34-4ff1-9b6d-eb1059334028"), "Processor performance increase threshold" },
                { new Guid("ea062031-0e34-4ff1-9b6d-eb1059334029"), "Processor performance increase threshold for Processor Power Efficiency Class 1" },
                { new Guid("f735a673-2066-4f80-a0c5-ddee0cf1bf5d"), "Processor Performance Core Parking Affinity History" },
                { new Guid("f8861c27-95e7-475c-865b-13c0cb3f9d6b"), "Processor Performance History Count for Processor Power Efficiency Class 0" },
                { new Guid("f8861c27-95e7-475c-865b-13c0cb3f9d6c"), "Processor Performance History Count for Processor Power Efficiency Class 1" },
                { new Guid("fddc842b-8364-4edc-94cf-c17f60de1c80"), "Processor Performance Core Parking Concurrency Threshold" },
                { new Guid("17aaa29b-8b43-4b94-aafe-35f64daaf1ee"), "Processor idle time check" },
                { new Guid("3c0bc021-c8a8-4e07-a973-6b14cbcb2b7e"), "Processor performance decrease time" },
                { new Guid("684c3e69-a4f7-4014-8754-d45179a56167"), "Processor performance core parking distribution threshold" },
                { new Guid("8ec4b3a5-6868-48c2-be75-4f3044be88a7"), "Processor performance core parking increase time" },
                { new Guid("90959d22-d6a1-49b9-af93-bce885ad335b"), "Processor performance core parking decrease time" },
                { new Guid("a9ceb8da-cd46-44fb-a98b-02af69de4623"), "Processor performance core parking affinity history" },
                { new Guid("aded5e82-b909-4619-9949-f5d71dac0bcb"), "Processor performance core parking parked performance state" },
                { new Guid("f1fbfde2-a960-4165-9f88-50667911ce96"), "Processor performance increase time" },
                { new Guid("fbd9aa66-9553-4097-ba44-ed6e9d65eab8"), "Processor performance core parking min cores" },
                { new Guid("0a7d6ab6-ac83-4ad1-8282-eca5b58308f3"), "Processor performance core parking overutilization threshold" },
                { new Guid("468fe7e5-1158-46ec-88bc-5b96c9e44fd0"), "Processor performance core parking overutilization weighting" },
                { new Guid("49cb11a5-56e2-4afb-9d38-3df47872e21b"), "Processor performance core parking utility distribution threshold" },
                { new Guid("5adbbfbc-074e-4da1-ba38-db8b36b2c8f3"), "Processor performance decrease threshold" },
                { new Guid("60c07fe1-0556-45cf-9903-d56e32210242"), "Processor performance increase policy" },
                { new Guid("61f45dfe-1919-4180-bb46-8cc70e0b38f1"), "Processor performance core parking concurrency headroom threshold" },
                { new Guid("82011705-fb95-4d46-8d35-4042b1d20def"), "Processor performance core parking increase policy" },
                { new Guid("9fe527be-1b70-48da-930d-7bcf17b44990"), "Processor performance core parking decrease policy" },
                { new Guid("a79c8e0e-f271-482d-8f8a-5db9a18312de"), "Processor performance core parking max cores for Processor Power Efficiency Class 0" },
                { new Guid("aca8648e-c4b1-4baa-8cce-9390ad647f8c"), "Processor performance core parking min cores for Processor Power Efficiency Class 0" },
                { new Guid("c763ee92-71e8-4127-84eb-f6ed043a3e3d"), "Processor performance history count" },
                { new Guid("cf8c6097-12b8-4279-bbdd-44601ee5209d"), "Processor idle state maximum for Processor Power Efficiency Class 1" },
                { new Guid("ee16691e-6ab3-4619-bb48-1c77c9357e5a"), "Processor idle disable for Processor Power Efficiency Class 1" },
                { new Guid("03680956-93bc-4294-bba6-4e0f09bb717f"), "Processor idle time check for Processor Power Efficiency Class 1" },
                { new Guid("10778347-1370-4ee0-8bbd-33bdacaade49"), "Processor performance decrease time for Processor Power Efficiency Class 1" },
                { new Guid("34c7b99f-9a6d-4b3c-8dc7-b6693b78cef4"), "Processor idle promote threshold for Processor Power Efficiency Class 1" },
                { new Guid("13d09884-f74e-474a-a852-b6bde8ad03a8"), "Processor idle demote threshold for Processor Power Efficiency Class 1" },
                { new Guid("5c5bb349-ad29-4ee2-9d0b-2b25270f7a81"), "Processor performance core parking distribution threshold for Processor Power Efficiency Class 1" },
                { new Guid("e69653ca-cf7f-4f05-aa73-cb833fa90ad4"), "Minimum processor state" },
                { new Guid("5dbb7c9f-38e9-40d2-9749-4f8a0e9f640f"), "Processor idle state maximum for Processor Power Efficiency Class 0" },
                { new Guid("637ea02f-bbcb-4015-8e2c-a1c7b9c0b546"), "Processor idle disable for Processor Power Efficiency Class 0" },
                { new Guid("8183ba9a-e910-48da-8769-14ae6dc1170a"), "Processor idle time check for Processor Power Efficiency Class 0" },
                { new Guid("9a66d8d7-4ff7-4ef9-b5a2-5a326ca2a469"), "Processor performance decrease time for Processor Power Efficiency Class 0" },
                { new Guid("bcded951-187b-4d05-bccc-f7e51960c258"), "Processor idle promote threshold for Processor Power Efficiency Class 0" },
                { new Guid("d8742dcb-3e6a-4b3c-b3fe-374623cdcf06"), "Processor idle demote threshold for Processor Power Efficiency Class 0" },
                { new Guid("f3c5027d-cd16-4930-aa6b-90db844a8f00"), "Processor performance core parking distribution threshold for Processor Power Efficiency Class 0" }
            };

        /// <summary>
        /// Known power subgroup GUID to alias mapping
        /// </summary>
        private static readonly Dictionary<Guid, string> _knownSubgroups = new Dictionary<Guid, string>
        {
            // Core subgroups
            { new Guid("238c9fa8-0aad-41ed-83f4-97be242c8f20"), "Sleep" },
            { new Guid("4f971e89-eebd-4455-a8de-9e59040e7347"), "Power Buttons and Lid" },
            { new Guid("0012ee47-9041-4b5d-9b77-535fba8b1442"), "Hard Disk" },
            { new Guid("7516b95f-f776-4464-8c53-06167f40cc99"), "Display" },
            { new Guid("54533251-82be-4824-96c1-47b60b740d00"), "Processor Power Management" },
            { new Guid("501a4d13-42af-4429-9fd1-a8218c268e20"), "PCI Express" },
            { new Guid("19cbb8fa-5279-450e-9fac-8a3d5fedd0c1"), "Wireless Adapter Settings" },
            { new Guid("9596fb26-9850-41fd-ac3e-f7c3c00afd4b"), "Multimedia Settings" },
            { new Guid("5fb4938d-1ee8-4b0f-9a3c-5036b0ab995c"), "Graphics" },
            { new Guid("e73a048d-bf27-4f12-9731-8b2076e8891f"), "Battery" },

            // Additional subgroups
            { new Guid("02f815b5-a5cf-4c84-bf20-649d1f75d3d8"), "Internet Explorer" },
            { new Guid("0d7dbae2-4294-402a-ba8e-26777e8488cd"), "Desktop Background Settings" },
            { new Guid("2a737441-1930-4402-8d77-b2bebba308a3"), "USB Settings" },
            { new Guid("44f3beca-a7c0-460e-9df2-bb8b99e0cba6"), "Intel(R) Graphics Settings" },
            { new Guid("48672f38-7a9a-4bb2-8bf8-3d85be19de4e"), "Power Settings" },
            { new Guid("2e601130-5351-4d9d-8e04-252966bad054"), "Energy Saver Settings" },
            { new Guid("8619b916-e004-4dd8-9b66-dae86f806698"), "AHCI Link Power Management" },
            { new Guid("de830923-a562-41af-a086-e3a2c6bad2da"), "Advanced Power Settings" }
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
            return _knownPowerSettings.TryGetValue(settingGuid, out string alias) ? alias : settingGuid.ToString();
        }

        /// <summary>
        /// Gets the friendly alias for a power subgroup GUID
        /// </summary>
        /// <param name="subgroupGuid">The GUID of the power subgroup</param>
        /// <returns>The friendly alias if known, or the GUID string if not</returns>
        public static string GetSubgroupAlias(Guid subgroupGuid)
        {
            return _knownSubgroups.TryGetValue(subgroupGuid, out string alias) ? alias : subgroupGuid.ToString();
        }

        /// <summary>
        /// Gets the GUID for a power setting alias
        /// </summary>
        /// <param name="settingAlias">The alias of the power setting</param>
        /// <returns>The GUID if found, or Guid.Empty if not</returns>
        public static Guid GetPowerSettingGuid(string settingAlias)
        {
            if (string.IsNullOrEmpty(settingAlias))
                return Guid.Empty;

            var match = _knownPowerSettings.FirstOrDefault(kvp =>
                kvp.Value.Equals(settingAlias, StringComparison.OrdinalIgnoreCase));

            return match.Key != Guid.Empty ? match.Key : Guid.Empty;
        }

        /// <summary>
        /// Gets the GUID for a power subgroup alias
        /// </summary>
        /// <param name="subgroupAlias">The alias of the power subgroup</param>
        /// <returns>The GUID if found, or Guid.Empty if not</returns>
        public static Guid GetSubgroupGuid(string subgroupAlias)
        {
            if (string.IsNullOrEmpty(subgroupAlias))
                return Guid.Empty;

            var match = _knownSubgroups.FirstOrDefault(kvp =>
                kvp.Value.Equals(subgroupAlias, StringComparison.OrdinalIgnoreCase));

            return match.Key != Guid.Empty ? match.Key : Guid.Empty;
        }

        #endregion
    }
}
