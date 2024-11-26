using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Azure.AI.OpenAI;
using Microsoft.Extensions.AI;
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
        /// This class needs a ChatClient to work.
        /// Pass your pre-configured ChatClient to this constructor.
        /// </summary>
        public ScenarioAssert(IChatClient chatClient, Action<string>? onLog = null)
        {
            Semantic = new Semantic(chatClient);
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
