﻿using Azure.AI.OpenAI;
using Microsoft.Extensions.AI;
using skUnit;
using skUnit.Scenarios;
using Xunit.Abstractions;

namespace Demo.TddShop.Test.BrainTests.ShopBrain
{
    public class ShopBrainTests
    {
        ScenarioAssert ScenarioAssert { get; set; }

        public ShopBrainTests(ITestOutputHelper output)
        {
            var deployment = Environment.GetEnvironmentVariable("openai-deployment-name")!;
            var azureKey = Environment.GetEnvironmentVariable("openai-api-key")!;
            var endpoint = Environment.GetEnvironmentVariable("openai-endpoint")!;

            var chatClient = new AzureOpenAIClient(
                new Uri(endpoint),
                new System.ClientModel.ApiKeyCredential(azureKey)
            ).AsChatClient(deployment);

            ScenarioAssert = new ScenarioAssert(chatClient, output.WriteLine);
        }

        [Theory]
        [InlineData("AskMenu_Happy")]
        [InlineData("AskMenu_Sad")]
        [InlineData("AskMenu_Angry")]
        [InlineData("AskMenu_Sad_Happy")]
        public async Task AskMenu_MustWork(string scenario)
        {
            var scenarioText = await File.ReadAllTextAsync(@$"BrainTests/ShopBrain/Scenarios/{scenario}.md");
            var scenarios = ChatScenario.LoadFromText(scenarioText, "");

            var chatClient = new TddShop.ShopBrain().CreateChatClient();

            await ScenarioAssert.PassAsync(scenarios, chatClient);
        }
    }
}