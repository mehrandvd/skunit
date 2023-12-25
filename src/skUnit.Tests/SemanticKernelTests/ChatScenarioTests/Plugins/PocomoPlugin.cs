using System.ComponentModel;
using Microsoft.SemanticKernel;

namespace skUnit.Tests.SemanticKernelTests.ChatScenarioTests.Plugins
{
    public class PocomoPlugin
    {
        [KernelFunction]
        [Description("Gets the pocomo price")]
        public string GetPocomoPrice()
        {
            return "112$";
        }
    }
}
