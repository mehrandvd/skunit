using System.Reflection;
using Azure.AI.OpenAI;
using Markdig.Helpers;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using skUnit.Scenarios;
using skUnit.Scenarios.Parsers;
using Xunit.Abstractions;

namespace skUnit.Tests.Infrastructure;

public class SemanticTestBase
{
    protected readonly string ApiKey;
    protected readonly string Endpoint;
    protected readonly string DeploymentName;
    protected IChatClient BaseChatClient { get; set; }
    protected ScenarioAssert ScenarioAssert { get; set; }
    protected ITestOutputHelper Output { get; set; }

    public SemanticTestBase(ITestOutputHelper output)
    {
        Output = output;
        var builder = new ConfigurationBuilder()
            .AddUserSecrets<SemanticTestBase>();

        IConfiguration configuration = builder.Build();

        ApiKey =
            configuration["AzureOpenAI_ApiKey"] ??
            throw new Exception("No ApiKey is provided.");
        Endpoint =
            configuration["AzureOpenAI_Endpoint"] ??
            throw new Exception("No Endpoint is provided.");
        DeploymentName =
            configuration["AzureOpenAI_Deployment"] ??
            throw new Exception("No Deployment is provided.");

        ScenarioAssert = new ScenarioAssert(
            new AzureOpenAIClient(
                new Uri(Endpoint),
                new System.ClientModel.ApiKeyCredential(ApiKey)
                ).GetChatClient(DeploymentName).AsIChatClient()
            , message => Output.WriteLine(message));

        var openAI = new AzureOpenAIClient(
            new Uri(Endpoint),
            new System.ClientModel.ApiKeyCredential(ApiKey)
        ).GetChatClient(DeploymentName).AsIChatClient();

        BaseChatClient = new ChatClientBuilder(openAI)
            .Build();
    }

    protected async Task<List<ChatScenario>> LoadChatScenarioAsync(string scenario)
    {
        return await ChatScenario.LoadFromResourceAsync($"skUnit.Tests.ScenarioAssertTests.Samples.{scenario}.md", Assembly.GetExecutingAssembly());
    }
}