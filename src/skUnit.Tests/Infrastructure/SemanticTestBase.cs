using System.Reflection;
using Azure.AI.OpenAI;
using Markdig.Helpers;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using skUnit.Scenarios;
using skUnit.Scenarios.Parsers;
using skUnit.Tests.ScenarioAssertTests.ChatScenarioTests.Plugins;
using Xunit.Abstractions;

namespace skUnit.Tests.Infrastructure;

public class SemanticTestBase
{
    private readonly string _apiKey;
    private readonly string _endpoint;
    private readonly string _deploymentName;
    protected Kernel Kernel { get; set; }
    protected IChatClient ChatClient { get; set; }
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
        Kernel = CreateKernel();

        var openAI = new AzureOpenAIClient(
            new Uri(_endpoint),
            new System.ClientModel.ApiKeyCredential(_apiKey)
        ).GetChatClient(_deploymentName).AsIChatClient();

        ChatClient = new ChatClientBuilder(openAI)
            .UseFunctionInvocation()
            .Build();
    }

    Kernel CreateKernel()
    {
        var builder = Kernel.CreateBuilder();
        builder.AddAzureOpenAIChatCompletion(_deploymentName, new AzureOpenAIClient(new Uri(_endpoint), new System.ClientModel.ApiKeyCredential(_apiKey)));
        builder.Services.AddLogging(loggerBuilder =>
        {
            loggerBuilder.SetMinimumLevel(LogLevel.Trace).AddDebug();
            loggerBuilder.ClearProviders();
            loggerBuilder.AddConsole();
        });

        builder.Plugins.AddFromType<TimePlugin>();
        var kernel = builder.Build();
        return kernel;
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