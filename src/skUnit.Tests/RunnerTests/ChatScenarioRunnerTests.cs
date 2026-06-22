using System.ComponentModel;
using Microsoft.Extensions.AI;
using skUnit.Exceptions;
using skUnit.Tests.Infrastructure;

namespace skUnit.Tests.RunnerTests
{
    public class ChatScenarioRunnerTests(ITestOutputHelper output) : SemanticTestBase(output)
    {
        [Fact]
        public async Task EiffelAndMouseChat_MustWork()
        {
            var scenarios = await LoadChatScenarioAsync("EiffelAndMouseChat");
            await ScenarioRunner.RunAsync(scenarios, BaseChatClient);
        }

        [Fact]
        public async Task EiffelTallChatWrong_ShouldThrow()
        {
            var scenarios = await LoadChatScenarioAsync("EiffelTallChatWrong");

            await Assert.ThrowsAsync<SemanticAssertException>(async () =>
            {
                await ScenarioRunner.RunAsync(scenarios, BaseChatClient);
            });
        }


        [Fact]
        public async Task FunctionCall_MustWork()
        {
            var scenarios = await LoadChatScenarioAsync("GetFoodMenuChat");

            // Run without tool, should fail
            await Assert.ThrowsAsync<SemanticAssertException>(async () =>
            {
                await ScenarioRunner.RunAsync(scenarios, BaseChatClient, getAnswerFunc: async history =>
                {
                    var result = await BaseChatClient.GetResponseAsync(
                        history,
                        options: new ChatOptions
                        {
                            // No tool
                        });

                    var answer = result;
                    return answer;
                });
            });

            // Run with tool, should pass
            await ScenarioRunner.RunAsync(scenarios, BaseChatClient, getAnswerFunc: async history =>
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

            await ScenarioRunner.RunAsync(scenarios, chatClient);
        }

        [Fact]
        public async Task FunctionCallJsonWrong_ShouldThrow()
        {
            var scenarios = await LoadChatScenarioAsync("GetFoodMenuChatJsonWrong");

            var builder = new ChatClientBuilder(BaseChatClient)
                          .ConfigureOptions(options =>
                          {
                              options.Tools ??= [];
                              options.Tools.Add(AIFunctionFactory.Create(GetFoodMenu));
                          })
                          .UseFunctionInvocation()
                ;

            var chatClient = builder.Build();

            await Assert.ThrowsAsync<SemanticAssertException>(async () =>
            {
                await ScenarioRunner.RunAsync(scenarios, chatClient);
            });
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
