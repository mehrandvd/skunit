using Azure.AI.OpenAI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using skUnit;
using skUnit.Scenarios;

namespace Demo.MSTest;

[TestClass]
public class ChatScenarioTests
{
    private static IChatClient _chatClient = null!;
    private static ScenarioAssert ScenarioAssert { get; set; } = null!;

    public TestContext TestContext { get; set; } = null!;

    [ClassInitialize]
    public static void ClassInitialize(TestContext context)
    {
        var configuration = new ConfigurationBuilder()
            .AddUserSecrets<ChatScenarioTests>()
            .Build();

        var apiKey = configuration["AzureOpenAI_ApiKey"]
            ?? throw new InvalidOperationException("AzureOpenAI_ApiKey not found in user secrets");
        var endpoint = configuration["AzureOpenAI_Endpoint"]
            ?? throw new InvalidOperationException("AzureOpenAI_Endpoint not found in user secrets");
        var deployment = configuration["AzureOpenAI_Deployment"]
            ?? throw new InvalidOperationException("AzureOpenAI_Deployment not found in user secrets");

        _chatClient = new AzureOpenAIClient(new Uri(endpoint), new System.ClientModel.ApiKeyCredential(apiKey))
            .GetChatClient(deployment)
            .AsIChatClient();

        // This uses Console.WriteLine for class-level initialization
        ScenarioAssert = new ScenarioAssert(_chatClient, context.WriteLine);
    }

    [TestMethod]
    public async Task SimpleGreeting_ShouldPass()
    {
        // Create per-test instance with TestContext logging

        var scenarios = await ChatScenario.LoadFromResourceAsync(
            "Demo.MSTest.Scenarios.SimpleGreeting.md",
            typeof(ChatScenarioTests).Assembly);

        await ScenarioAssert.PassAsync(scenarios, _chatClient);
    }

    [TestMethod]
    public async Task GetCurrentTimeChat_ShouldPass()
    {
        var scenarios = await ChatScenario.LoadFromResourceAsync(
            "Demo.MSTest.Scenarios.GetCurrentTimeChat.md",
            typeof(ChatScenarioTests).Assembly);

        await ScenarioAssert.PassAsync(scenarios, _chatClient);
    }

    [TestMethod]
    public async Task JsonUserInfo_ShouldPass()
    {
        var scenarios = await ChatScenario.LoadFromResourceAsync(
            "Demo.MSTest.Scenarios.JsonUserInfo.md",
            typeof(ChatScenarioTests).Assembly);

        await ScenarioAssert.PassAsync(scenarios, _chatClient);
    }

    [DataTestMethod]
    [DataRow("SimpleGreeting")]
    [DataRow("GetCurrentTimeChat")]
    [DataRow("JsonUserInfo")]
    public async Task ScenarioMatrix_ShouldPass(string scenarioName)
    {
        var scenarios = await ChatScenario.LoadFromResourceAsync(
            $"Demo.MSTest.Scenarios.{scenarioName}.md",
            typeof(ChatScenarioTests).Assembly);

        await ScenarioAssert.PassAsync(scenarios, _chatClient);
    }
}