using System;
using System.Management.Automation;

namespace PowerPlanTools.Utils
{
    /// <summary>
    /// Helper class for logging in PowerPlanTools cmdlets
    /// </summary>
    internal static class LoggingHelper
    {
        /// <summary>
        /// Writes a verbose message with timestamp.
        /// </summary>
        /// <param name="cmdlet">The cmdlet instance to use for writing verbose output.</param>
        /// <param name="message">The message to write.</param>
        /// <param name="counter">Optional counter to include in the message.</param>
        /// <param name="total">Optional total for the counter.</param>
        public static void LogVerbose(PSCmdlet cmdlet, string message, int? counter = null, int? total = null)
        {
            // Only write verbose messages if the -Verbose parameter is specified
            if (cmdlet.MyInvocation.BoundParameters.ContainsKey("Verbose") &&
                cmdlet.MyInvocation.BoundParameters["Verbose"].Equals(true))
            {
                // Format the message with timestamp
                string timestamp = DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss.fff");
                string formattedMessage;

                if (counter.HasValue && total.HasValue)
                {
                    string counterText = $"[{counter.Value}/{total.Value}]";
                    formattedMessage = $"{timestamp} - {counterText} - {message}";
                }
                else
                {
                    formattedMessage = $"{timestamp} - {message}";
                }

                // Write the verbose message
                cmdlet.WriteVerbose(formattedMessage);
            }
        }
    }
}
