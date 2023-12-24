using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.SemanticKernel;
using SemanticValidation;
using skUnit.Models;

namespace skUnit
{
    public class SemanticKernelAssert
    {
        private static Semantic? _semantic;

        private static Semantic Semantic
        {
            get => _semantic ?? throw new InvalidOperationException("KernelAssert has not initialized yet.");
            set => _semantic = value;
        }

        public static void Initialize(string endpoint, string apiKey)
        {
            Semantic = new Semantic(endpoint, apiKey);

        }
        
        public static async Task TestScenarioOnFunction(Kernel kernel, KernelFunction function, TextScenario test)
        {
            var arguments = new KernelArguments();
            foreach (var testArgument in test.Arguments)
            {
                arguments.Add(testArgument.Key, testArgument.Value);
            }

            var result = await function.InvokeAsync<string>(kernel, arguments);

            foreach (var kernelAssert in test.Asserts)
            {
                await kernelAssert.Assert(Semantic, result);
            }
        }
    }
}
