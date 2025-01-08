using System.ComponentModel;
using System.Globalization;
using Markdig.Helpers;
using Microsoft.SemanticKernel;

namespace skUnit.Tests.ScenarioAssertTests.ChatScenarioTests.Plugins
{
    public class TimePlugin
    {
        [KernelFunction]
        [Description("Gets the current time.")]
        public string GetCurrentTime()
        {
            return DateTime.Now.ToString(CultureInfo.InvariantCulture);
        }
    }
}
