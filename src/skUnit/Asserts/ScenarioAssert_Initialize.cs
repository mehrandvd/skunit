using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.SemanticKernel;
using SemanticValidation;
using skUnit.Exceptions;
using skUnit.Scenarios;
using skUnit.Scenarios.Parsers.Assertions;

namespace skUnit
{
    /// <summary>
    /// This class is for testing skUnit scenarios semantically. It contains various methods
    /// that you can test kernels and functions with scenarios. Scenarios are some markdown files with a specific format.
    /// </summary>
    public partial class ScenarioAssert
    {
        private Action<string>? OnLog { get; set; }
        private Semantic Semantic { get; set; }

        /// <summary>
        /// This class needs a SemanticKernel kernel to work.
        /// Using this constructor you can use an AzureOpenAI subscription to configure it.
        /// If you want to connect using an OpenAI client, you can configure your kernel
        /// as you like and pass your pre-configured kernel using the other constructor.
        /// </summary>
        /// <param name="deploymentName"></param>
        /// <param name="endpoint"></param>
        /// <param name="apiKey"></param>
        /// <param name="onLog">If you're using xUnit, do this in the constructor:
        /// <code>
        /// MyTest(ITestOutputHelper output)
        /// {
        ///    SemanticKernelAssert = new SemanticKernelAssert(_deploymentName, _endpoint, _apiKey, output.WriteLine);
        /// }
        /// </code>
        /// </param>
        public ScenarioAssert(string deploymentName, string endpoint, string apiKey, Action<string> onLog)
        {
            Semantic = new Semantic(deploymentName, endpoint, apiKey);
            OnLog = onLog;
        }

        /// <summary>
        /// This class needs a SemanticKernel kernel to work.
        /// Pass your pre-configured kernel to this constructor.
        /// </summary>
        /// <param name="kernel"></param>
        public ScenarioAssert(Kernel kernel, Action<string>? onLog = null)
        {
            Semantic = new Semantic(kernel);
            OnLog = onLog;

        }

        private void Log(string? message = "")
        {
            if (OnLog is not null)
            {
                OnLog(message);
            }
        }

        private async Task CheckAssertionAsync(IKernelAssertion assertion, string answer)
        {
            Log($"### CHECK {assertion.AssertionType}");
            Log($"{assertion.Description}");

            try
            {
                await assertion.Assert(Semantic, answer);
                Log($"✅ OK");
                Log("");
            }
            catch (SemanticAssertException exception)
            {
                Log("❌ FAIL");
                Log("Reason:");
                Log(exception.Message);
                Log();
                throw;
            }
        }
    }
}
