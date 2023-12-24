using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.SemanticKernel;
using SemanticValidation;
using skUnit.Scenarios;

namespace skUnit
{
    public class SemanticKernelAssert
    {
        private static Semantic? _semantic;
        private static Action<string>? OnLog { get; set; }
        private static Semantic Semantic
        {
            get => _semantic ?? throw new InvalidOperationException("KernelAssert has not initialized yet.");
            set => _semantic = value;
        }

        public static void Initialize(string endpoint, string apiKey, Action<string>? onLog = null)
        {
            Semantic = new Semantic(endpoint, apiKey);
            OnLog = onLog;
        }

        static void Log(string message)
        {
            if (OnLog is not null)
            {
                OnLog(message);
            }
        }
        
        public static async Task TestScenarioOnFunction(Kernel kernel, KernelFunction function, TextScenario scenario)
        {
            var arguments = new KernelArguments();
            Log($"# TEST {scenario.Description}");
            Log("");
            foreach (var argument in scenario.Arguments)
            {
                arguments.Add(argument.Key, argument.Value);
                Log($"## PARAMETER {argument.Key}");
                Log($"{argument.Value}");
            }

            var result = await function.InvokeAsync<string>(kernel, arguments);

            Log($"## ACTUAL ANSWER:");
            Log(result ?? "");
            Log("");

            foreach (var kernelAssert in scenario.Asserts)
            {
                Log($"## ANSWER {kernelAssert.AssertionType}");
                await kernelAssert.Assert(Semantic, result);
                Log($"OK");
            }
        }
    }
}
