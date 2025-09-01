﻿using Microsoft.Extensions.AI;
using skUnit.Tests.Infrastructure;
using System.ComponentModel;
using Xunit.Abstractions;

namespace skUnit.Tests.ScenarioAssertTests
{
    public class HallucinationChatClientTests : SemanticTestBase
    {

        public HallucinationChatClientTests(ITestOutputHelper output) : base(output)
        {

        }

        [Fact]
        public async Task EiffelTallChat_MustWork()
        {
            var scenarios = await LoadChatScenarioAsync("EiffelTallChat");
            await ScenarioRunner.RunAsync(
                scenarios,
                SystemUnderTestClient,
                options: new ScenarioRunOptions
                {
                    TotalRuns = 3,
                    MinSuccessRate = .6
                }
                );
        }


        [Fact]
        public async Task FunctionCall_MustWork()
        {
            var scenarios = await LoadChatScenarioAsync("GetFoodMenuChat");
            await ScenarioRunner.RunAsync(
                scenarios,
                SystemUnderTestClient,
                options: new ScenarioRunOptions
                {
                    TotalRuns = 3,
                    MinSuccessRate = .6
                },
                getAnswerFunc: async history =>
            {
                AIFunction getFoodMenu = AIFunctionFactory.Create(GetFoodMenu);

                var result = await SystemUnderTestClient.GetResponseAsync(
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

            var builder = new ChatClientBuilder(SystemUnderTestClient)
                .ConfigureOptions(options =>
                {
                    options.Tools ??= [];
                    options.Tools.Add(AIFunctionFactory.Create(GetFoodMenu));
                })
                .UseFunctionInvocation()
                ;

            var chatClient = builder.Build();

            await ScenarioRunner.RunAsync(scenarios, chatClient,
                options: new ScenarioRunOptions
                {
                    TotalRuns = 3,
                    MinSuccessRate = .6
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
