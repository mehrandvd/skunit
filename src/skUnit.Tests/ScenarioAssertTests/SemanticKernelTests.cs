using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.AI;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using skUnit.Tests.Infrastructure;
using skUnit.Tests.ScenarioAssertTests.Plugins;
using System.Net;
using Xunit.Abstractions;

namespace skUnit.Tests.ScenarioAssertTests
{
    public class SemanticKernelChatTests : SemanticTestBase
    {
        private Kernel Kernel { get; set; }
        public SemanticKernelChatTests(ITestOutputHelper output) : base(output)
        {

            var builder = Kernel.CreateBuilder().AddAzureOpenAIChatCompletion(DeploymentName, Endpoint, ApiKey);
            Kernel = builder.Build();

            // Add a plugin (the LightsPlugin class is defined below)
            Kernel.Plugins.AddFromType<TimePlugin>("TimePlugin");
        }

        [Fact, Experimental("SKEXP0001")]
        public async Task EiffelTallChat_MustWork()
        {
            var scenarios = await LoadChatScenarioAsync("EiffelTallChat");
            await ScenarioAssert.PassAsync(scenarios, Kernel);
        }

        [Fact, Experimental("SKEXP0001")]
        public async Task TimeFunctionCall_MustWork()
        {
            var scenarios = await LoadChatScenarioAsync("GetCurrentTimeChat");
            await ScenarioAssert.PassAsync(scenarios, Kernel);

            //await ScenarioAssert.PassAsync(scenarios, Kernel, getAnswerFunc: async chatHistory =>
            //{
            //    var chatService = Kernel.GetRequiredService<IChatCompletionService>();
            //    var result = await chatService.GetChatMessageContentsAsync(
            //        chatHistory,
            //        new PromptExecutionSettings { FunctionChoiceBehavior = FunctionChoiceBehavior.Auto() },
            //        kernel: Kernel
            //    );

            //    var answer = "";

            //    return new ChatResponse(new ChatMessage(ChatRole.Assistant, answer));
            //});
        }
    }


}