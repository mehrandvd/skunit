using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.SemanticKernel;

namespace skUnit.Tests.SemanticKernel.ChatScenarioTests.Plugins
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
