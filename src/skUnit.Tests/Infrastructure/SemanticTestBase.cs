using System.Reflection;
using Azure;
using Azure.AI.OpenAI;
using Markdig.Helpers;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using skUnit.Scenarios;
using skUnit.Scenarios.Parsers;

namespace skUnit.Tests.Infrastructure;

public class SemanticTestBase
{
    protected readonly string ApiKey;
    protected readonly string Endpoint;
    protected readonly string DeploymentName;
    protected IChatClient BaseChatClient { get; set; }
    protected ChatScenarioRunner ScenarioRunner { get; set; }
    protected SemanticAssertions SemanticAssertions { get; set; }

    protected ITestOutputHelper Output { get; set; }
    protected IConfiguration Configuration { get; set; }

    public SemanticTestBase(ITestOutputHelper output)
    {
        Output = output;
        var builder = new ConfigurationBuilder()
            .AddUserSecrets<SemanticTestBase>()
            .AddEnvironmentVariables();

        Configuration = builder.Build();

        ApiKey =
            Configuration["AzureOpenAI_ApiKey"] ??
            throw new Exception("No ApiKey is provided.");
        Endpoint =
            Configuration["AzureOpenAI_Endpoint"] ??
            throw new Exception("No Endpoint is provided.");
        DeploymentName =
            Configuration["AzureOpenAI_Deployment"] ??
            throw new Exception("No Deployment is provided.");

        // Create assertion client for semantic evaluations
        var assertionClient = new AzureOpenAIClient(
            new Uri(Endpoint),
            new AzureKeyCredential(ApiKey)
            ).GetChatClient(DeploymentName).AsIChatClient();

        var logger = new DelegateLoggerAdapter(Output.WriteLine);

        ScenarioRunner = new ChatScenarioRunner(assertionClient, logger);

        SemanticAssertions = new SemanticAssertions(assertionClient);

        // Create system under test client
        var openAI = new AzureOpenAIClient(
            new Uri(Endpoint),
            new AzureKeyCredential(ApiKey)
        ).GetChatClient(DeploymentName).AsIChatClient();

        BaseChatClient = new ChatClientBuilder(openAI)
            .Build();
    }

    protected async Task<IReadOnlyList<ChatScenario>> LoadChatScenarioAsync(string scenario)
    {
        return await ChatScenario.ParseFromResourceAsync($"skUnit.Tests.RunnerTests.Samples.{scenario}.md", Assembly.GetExecutingAssembly());
    }

    private sealed class NullScope : IDisposable
    {
        public static NullScope Instance { get; } = new();

        public void Dispose()
        {
        }
    }
}