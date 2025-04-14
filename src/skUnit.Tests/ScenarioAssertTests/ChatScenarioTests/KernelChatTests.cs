using Microsoft.Extensions.AI;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using skUnit.Tests.Infrastructure;
using Xunit.Abstractions;

namespace skUnit.Tests.ScenarioAssertTests.ChatScenarioTests
{
    public class KernelChatTests : SemanticTestBase
    {
        public KernelChatTests(ITestOutputHelper output) : base(output)
        {
            var func = Kernel.CreateFunctionFromPrompt("""
                [[INPUT]]
                {{$input}}
                [[END OF INPUT]]
                
                Get intent of input. Intent should be one of these options: {{$options}}.

                INTENT:
                """, new PromptExecutionSettings(), "GetIntent");
            Kernel.Plugins.AddFromFunctions("MyPlugin", "", new[] { func });
        }

        [Fact]
        public async Task EiffelTallChat_MustWork()
        {
            var scenarios = await LoadChatScenarioAsync("EiffelTallChat");
            await ScenarioAssert.PassAsync(scenarios, Kernel);
        }

        [Fact]
        public async Task TimeFunctionCall_MustWork()
        {
            var scenarios = await LoadChatScenarioAsync("GetCurrentTimeChat");
            await ScenarioAssert.PassAsync(scenarios, Kernel, getAnswerFunc: async chatHistory =>
            {
                var chatService = Kernel.GetRequiredService<IChatCompletionService>();
                var result = await chatService.GetChatMessageContentsAsync(
                    chatHistory,
                    new PromptExecutionSettings { FunctionChoiceBehavior = FunctionChoiceBehavior.Auto() },
                    kernel: Kernel
                    );

                var answer = "";

                return new ChatResponse(new ChatMessage(ChatRole.Assistant, answer));
            });
        }
    }

    
}
