using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.SemanticKernel;
using SemanticValidation;
using skUnit.Exceptions;
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

        public static void Initialize(string deploymentName, string endpoint, string apiKey, Action<string>? onLog = null)
        {
            Semantic = new Semantic(deploymentName, endpoint, apiKey);
            OnLog = onLog;
        }

        static void Log(string message)
        {
            if (OnLog is not null)
            {
                OnLog(message);
            }
        }
        
        public static async Task ScenarioSuccessAsync(Kernel kernel, KernelFunction function, TextScenario scenario)
        {
            var arguments = new KernelArguments();
            Log($"# TEST {scenario.Description}");
            Log("");
            foreach (var parameter in scenario.Parameters)
            {
                arguments.Add(parameter.Key, parameter.Value);
                Log($"## PARAMETER {parameter.Key}");
                Log($"{parameter.Value}");
            }

            var result = await function.InvokeAsync<string>(kernel, arguments);

            Log($"## ACTUAL ANSWER:");
            Log(result ?? "");
            Log("");

            foreach (var kernelAssert in scenario.Assertions)
            {
                Log($"## ANSWER {kernelAssert.AssertionType}");
                Log($"{kernelAssert.Description}");
                await kernelAssert.Assert(Semantic, result);
                Log($"OK");
            }
        }

        public static async Task ScenarioSuccessAsync(Kernel kernel, KernelFunction function,
            List<TextScenario> scenarios)
        {
            foreach (var scenario in scenarios)
            {
                await ScenarioSuccessAsync(kernel, function, scenario);
                Log("");
                Log("----------------------------------");
                Log("");
            }
        }

        public static async Task ScenarioThrowsAsync<TSemanticAssertException>(Kernel kernel, KernelFunction function, TextScenario scenario) where TSemanticAssertException : SemanticAssertException
        {
            var isThrown = false;
            try
            {
                var arguments = new KernelArguments();
                Log($"# TEST {scenario.Description}");
                Log("");
                foreach (var parameters in scenario.Parameters)
                {
                    arguments.Add(parameters.Key, parameters.Value);
                    Log($"## PARAMETER {parameters.Key}");
                    Log($"{parameters.Value}");
                }

                var result = await function.InvokeAsync<string>(kernel, arguments);

                Log($"## ACTUAL ANSWER:");
                Log(result ?? "");
                Log("");

                foreach (var assertion in scenario.Assertions)
                {
                    Log($"## ANSWER {assertion.AssertionType}");
                    Log($"{assertion.Description}");
                    await assertion.Assert(Semantic, result);
                    Log($"OK");
                }
            }
            catch (SemanticAssertException exception)
            {
                Log("Exception as EXPECTED:");
                Log(exception.Message);
                isThrown = true;
            }

            if (!isThrown)
            {
                throw new Exception($"Expected for an exception of type: {typeof(TSemanticAssertException).FullName}");
            }
            
        }

        public static async Task ScenarioThrowsAsync<TSemanticAssertException>(Kernel kernel, KernelFunction function,
            List<TextScenario> scenarios) where TSemanticAssertException : SemanticAssertException
        {
            foreach (var scenario in scenarios)
            {
                await ScenarioThrowsAsync<TSemanticAssertException>(kernel, function, scenario);
                Log("");
                Log("----------------------------------");
                Log("");
            }
        }
    }
}
