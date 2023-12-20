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

        public static async Task<SemanticAssertResult> AreSameCoreAsync(string first, string second)
        {
            var skresult = (
                await AreSameSkFunc.InvokeAsync(TestKernel, new KernelArguments()
                {
                    ["first_text"] = first,
                    ["second_text"] = second
                })
            ).GetValue<string>() ?? "";

            var result = JsonSerializer.Deserialize<SemanticAssertResult>(skresult);

            if (result is null)
                throw new InvalidOperationException("Can not assert AreSame");

            return result;
        }

        public static async Task AreSameAsync(string first, string second)
        {
            var result = await AreSameCoreAsync(first, second);

            if (result is null)
            {
                throw new SemanticAssertException("Unable to accomplish the semantic assert.");
            }

            if (!result.Success)
            {
                throw new SemanticAssertException(result.Message);
            }
        }

        public static void AreSame(string first, string second)
        {
            AreSameAsync(first, second).GetAwaiter().GetResult();
        }

        public static async Task AreNotSameAsync(string first, string second)
        {
            var result = await AreSameCoreAsync(first, second);

            if (result is null)
            {
                throw new SemanticAssertException("Unable to accomplish the semantic assert.");
            }

            if (result.Success)
            {
                throw new SemanticAssertException($"""
                    These are semantically same:
                    [FIRST]: {first}
                    [SECOND]: {second} 
                    """);
            }
        }

        public static void AreNotSame(string first, string second)
        {
            AreNotSameAsync(first, second).GetAwaiter().GetResult();
        }


        public static void Satisfies(string input, string condition)
        {

        }
    }
}
