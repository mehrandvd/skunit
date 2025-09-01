using Azure.AI.OpenAI;
using Microsoft.Extensions.AI;
using skUnit;
using skUnit.Scenarios;
using Xunit.Abstractions;

namespace Demo.TddShop.Test.BrainTests.ShopBrain
{
    public class ShopBrainTests
    {
        ChatScenarioRunner ScenarioRunner { get; set; }
        IChatClient systemUnderTestClient { get; set; }

        public ShopBrainTests(ITestOutputHelper output)
        {
            var deployment = Environment.GetEnvironmentVariable("AzureOpenAI_Gpt4_Deployment")!;
            var azureKey = Environment.GetEnvironmentVariable("AzureOpenAI_Gpt4_ApiKey")!;
            var endpoint = Environment.GetEnvironmentVariable("AzureOpenAI_Gpt4_Endpoint")!;

            var assertionClient = new AzureOpenAIClient(
                new Uri(endpoint),
                new System.ClientModel.ApiKeyCredential(azureKey)
            ).GetChatClient(deployment).AsIChatClient();

            ScenarioRunner = new ChatScenarioRunner(assertionClient, output.WriteLine);
            systemUnderTestClient = new TddShop.ShopBrain().CreateChatClient();
        }

        [Theory]
        [InlineData("AskMenu_Happy")]
        [InlineData("AskMenu_Sad")]
        [InlineData("AskMenu_Angry")]
        [InlineData("AskMenu_Sad_Happy")]
        public async Task AskMenu_MustWork(string scenario)
        {
            var scenarioText = await File.ReadAllTextAsync(@$"BrainTests/ShopBrain/Scenarios/{scenario}.md");
            var scenarios = ChatScenario.LoadFromText(scenarioText);

            await ScenarioRunner.RunAsync(scenarios, systemUnderTestClient);
        }
    }
}
