using Microsoft.SemanticKernel;
using System.Text.Json;
using SemanticValidation.Models;

namespace SemanticValidation
{
    public static class Semantic
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

        public static async Task<SemanticValidationResult> AreSameAsync(string first, string second)
        {
            var skresult = (
                await AreSameSkFunc.InvokeAsync(TestKernel, new KernelArguments()
                {
                    ["first_text"] = first,
                    ["second_text"] = second
                })
            ).GetValue<string>() ?? "";

            var result = JsonSerializer.Deserialize<SemanticValidationResult>(skresult);

            if (result is null)
                throw new InvalidOperationException("Can not assert Same");

            return result;
        }

        public static void Satisfies(string input, string condition)
        {

        }
    }
}
