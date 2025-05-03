using System;
using System.Collections.Generic;
using System.Management.Automation;
using PowerPlanTools.Models;

namespace PowerPlanTools.Cmdlets
{
    /// <summary>
    /// Test cmdlet for debugging
    /// </summary>
    [Cmdlet(VerbsDiagnostic.Test, "PowerPlanTools")]
    public class TestPowerPlanToolsCmdlet : PSCmdlet
    {
        /// <summary>
        /// Process the cmdlet
        /// </summary>
        protected override void ProcessRecord()
        {
            try
            {
                WriteObject("PowerPlanTools test cmdlet is working!");

                // Create a test power setting
                var setting = new PowerSetting
                {
                    Alias = "Test Setting",
                    SettingGuid = Guid.NewGuid(),
                    SubGroupGuid = Guid.NewGuid(),
                    Description = "Test description",
                    PluggedIn = 1,
                    OnBattery = 0,
                    Units = "%",
                    IsHidden = false
                };

                WriteObject(setting);
            }
            catch (Exception ex)
            {
                WriteError(new ErrorRecord(ex, "TestPowerPlanToolsError", ErrorCategory.NotSpecified, null));
            }
        }
    }
}
