using Microsoft.Extensions.AI;
using Microsoft.SemanticKernel;
using skUnit.Tests.Infrastructure;
using Xunit.Abstractions;

namespace skUnit.Tests.ScenarioAssertTests.ChatScenarioTests
{
    public class ChatClientTests : SemanticTestBase
    {
        public ChatClientTests(ITestOutputHelper output) : base(output)
        {
            
        }

        [Fact]
        public async Task EiffelTallChat_MustWork()
        {
            var scenarios = await LoadChatScenarioAsync("EiffelTallChat");
            await ScenarioAssert.PassAsync(scenarios, ChatClient);
        }


        [Fact]
        public async Task FunctionCall_MustWork()
        {
            var scenarios = await LoadChatScenarioAsync("GetCurrentTimeChat");
            await ScenarioAssert.PassAsync(scenarios, ChatClient, getAnswerFunc: async history =>
            {
                AIFunction getCurrentTime = AIFunctionFactory.Create(GetCurrentTime);

                var result = await ChatClient.CompleteAsync(
                    history,
                    options: new ChatOptions
                    {
                        Tools = [getCurrentTime]
                    });

                var answer =  result.Choices.First().Text ?? "";
                return answer;
            });
        }

        [Fact]
        public async Task FunctionCallJson_MustWork()
        {
            var scenarios = await LoadChatScenarioAsync("GetCurrentTimeChatJson");
            await ScenarioAssert.PassAsync(scenarios, ChatClient, getAnswerFunc: async history =>
            {
                AIFunction getCurrentTime = AIFunctionFactory.Create(GetCurrentTime);

                var result = await ChatClient.CompleteAsync(
                    history,
                    options: new ChatOptions
                    {
                        Tools = [getCurrentTime]
                    });

                var answer = result.Choices.First().Text ?? "";
                return answer;
            });
        }


        private static string GetCurrentTime()
        {
            return DateTime.Now.ToString("HH:mm:ss");
        }
    }

    
}
