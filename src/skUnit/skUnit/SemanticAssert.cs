using System.Text.Json;
using System.Text.Json.Nodes;
using Json.More;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using skUnit.Exceptions;
using skUnit.Models;

namespace skUnit
{
    public class SemanticAssert
    {
        private static Kernel? _testKernel;
        private static KernelFunction _areSameSKFunc;
        private static Kernel TestKernel
        {
            get => _testKernel ?? throw new InvalidOperationException("SemanticAssert is not initialized yet.");
            set => _testKernel = value;
        }

        public static KernelFunction AreSameSkFunc
        {
            get => _areSameSKFunc ?? throw new InvalidOperationException("SemanticAssert is not initialized yet.");
            set => _areSameSKFunc = value;
        }

        public static void Initialize(string endpoint, string apiKey)
        {
            var builder = Kernel.CreateBuilder();
            builder.AddAzureOpenAIChatCompletion("gpt-35-turbo-test", endpoint, apiKey);

            TestKernel = builder.Build();

            var dir = Environment.CurrentDirectory;

            var testPlugin = TestKernel.CreatePluginFromPromptDirectory(Path.Combine(dir, "Plugins", "TestPlugin"));

            AreSameSkFunc = testPlugin["AreSame"];

        }


        public static async Task AreSameAsync(string first, string second)
        {
            var result = (
                await AreSameSkFunc.InvokeAsync(TestKernel, new KernelArguments()
                {
                    ["first_text"] = first,
                    ["second_text"] = second
                })
            ).GetValue<string>() ?? "";

            var assert = JsonSerializer.Deserialize<SemanticAssertResult>(result);

            if (assert is null)
            {
                throw new SemanticAssertException("Unable to accomplish the semantic assert.");
            }

            if (!assert.Success)
            {
                throw new SemanticAssertException(assert.Message);
            }
        }

        public static void AreSame(string first, string second)
        {
            AreSameAsync(first, second).GetAwaiter().GetResult();
        }

        public static void Satisfies(string input, string condition)
        {

        }
    }
}
