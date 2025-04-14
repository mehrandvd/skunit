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
    private readonly string _apiKey;
    private readonly string _endpoint;
    private readonly string _deploymentName;
    protected IChatClient BaseChatClient { get; set; }
    protected ScenarioAssert ScenarioAssert { get; set; }
    protected ITestOutputHelper Output { get; set; }

    public SemanticTestBase(ITestOutputHelper output)
    {
        Output = output;
        var builder = new ConfigurationBuilder()
            .AddUserSecrets<SemanticTestBase>();

        IConfiguration configuration = builder.Build();

        _apiKey =
            configuration["AzureOpenAI_ApiKey"] ??
            throw new Exception("No ApiKey is provided.");
        _endpoint =
            configuration["AzureOpenAI_Endpoint"] ??
            throw new Exception("No Endpoint is provided.");
        _deploymentName =
            configuration["AzureOpenAI_Deployment"] ??
            throw new Exception("No Deployment is provided.");

        ScenarioAssert = new ScenarioAssert(
            new AzureOpenAIClient(
                new Uri(_endpoint),
                new System.ClientModel.ApiKeyCredential(_apiKey)
                ).GetChatClient(_deploymentName).AsIChatClient()
            , message => Output.WriteLine(message));

        var openAI = new AzureOpenAIClient(
            new Uri(_endpoint),
            new System.ClientModel.ApiKeyCredential(_apiKey)
        ).GetChatClient(_deploymentName).AsIChatClient();

        BaseChatClient = new ChatClientBuilder(openAI)
            .Build();
    }

    protected async Task<List<InvocationScenario>> LoadInvokeScenarioAsync(string scenario)
    {
        return await InvocationScenario.LoadFromResourceAsync($"skUnit.Tests.ScenarioAssertTests.InvokeScenarioTests.Samples.{scenario}.sktest.md", Assembly.GetExecutingAssembly());
    }

    protected async Task<List<ChatScenario>> LoadChatScenarioAsync(string scenario)
    {
        return await ChatScenario.LoadFromResourceAsync($"skUnit.Tests.ScenarioAssertTests.ChatScenarioTests.Samples.{scenario}.md", Assembly.GetExecutingAssembly());
    }
}