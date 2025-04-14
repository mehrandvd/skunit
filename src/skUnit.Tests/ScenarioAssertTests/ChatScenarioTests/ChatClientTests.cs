using Microsoft.Extensions.AI;
using Microsoft.SemanticKernel;
using skUnit.Tests.Infrastructure;
using System.ComponentModel;
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
            await ScenarioAssert.PassAsync(scenarios, BaseChatClient);
        }


        [Fact]
        public async Task FunctionCall_MustWork()
        {
            var scenarios = await LoadChatScenarioAsync("GetFoodMenuChat");
            await ScenarioAssert.PassAsync(scenarios, BaseChatClient, getAnswerFunc: async history =>
            {
                AIFunction getFoodMenu = AIFunctionFactory.Create(GetFoodMenu);

                var result = await BaseChatClient.GetResponseAsync(
                    history,
                    options: new ChatOptions
                    {
                        Tools = [getFoodMenu]
                    });

                var answer = result;
                return answer;
            });
        }

        [Fact]
        public async Task FunctionCallJson_MustWork()
        {
            var scenarios = await LoadChatScenarioAsync("GetFoodMenuChatJson");

            var builder = new ChatClientBuilder(BaseChatClient)
                .ConfigureOptions(options =>
                {
                    options.Tools ??= [];
                    options.Tools.Add(AIFunctionFactory.Create(GetFoodMenu));
                })
                .UseFunctionInvocation()
                ;

            var chatClient = builder.Build();

            await ScenarioAssert.PassAsync(scenarios, chatClient);
        }


        [Description("Gets a food menu based on the user's mood")]
        private static string GetFoodMenu(
            [Description("User's mood based on its chat hsitory.")]
            UserMood mood
            )
        {
            return mood switch
            {
                UserMood.Happy => "Pizza",
                UserMood.Sad => "Ice Cream",
                UserMood.Angry => "Hot Dog",
                _ => "Nothing"
            };
        }

        enum UserMood
        {
            Happy,
            Sad,
            Angry
        }
    }

    
}
