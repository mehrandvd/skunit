using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using skUnit.Scenarios;
using skUnit.Scenarios.Parsers;
using Xunit.Abstractions;

namespace skUnit.Tests.SemanticKernel.TextScenarioTests;

public class SemanticTestBase
{
    protected string _apiKey;
    protected string _endpoint;
    protected string _deploymentName;
    protected Kernel Kernel { get; set; }
    protected ITestOutputHelper Output { get; set; }

    public SemanticTestBase(ITestOutputHelper output)
    {
        Output = output;
        _apiKey = Environment.GetEnvironmentVariable("openai-api-key", EnvironmentVariableTarget.User) ??
                  throw new Exception("No ApiKey in environment variables.");
        _endpoint = Environment.GetEnvironmentVariable("openai-endpoint", EnvironmentVariableTarget.User) ??
                    throw new Exception("No Endpoint in environment variables.");
        _deploymentName =
            Environment.GetEnvironmentVariable("openai-deployment-name", EnvironmentVariableTarget.User) ??
            throw new Exception("No DeploymentName in environment variables.");

        SemanticKernelAssert.Initialize(_deploymentName, _endpoint, _apiKey, message => Output.WriteLine(message));
        Kernel = CreateKernel();
    }

    Kernel CreateKernel()
    {
        var builder = Kernel.CreateBuilder();
        builder.AddAzureOpenAIChatCompletion(_deploymentName, _endpoint, _apiKey);
        builder.Services.AddLogging(loggerBuilder =>
        {
            loggerBuilder.SetMinimumLevel(LogLevel.Trace).AddDebug();
            loggerBuilder.ClearProviders();
            loggerBuilder.AddConsole();
        });
        var kernel = builder.Build();
        return kernel;
    }

    protected async Task<List<TextScenario>> LoadTextScenarioAsync(string scenario)
    {
        var testContent = await LoadTextTestAsync(scenario);
        var scenarios = TextScenarioParser.Parse(testContent, "");
        return scenarios;
    }

    private async Task<string> LoadTextTestAsync(string scenario)
    {
        var assembly = Assembly.GetExecutingAssembly();
        var resourceName = $"skUnit.Tests.SemanticKernel.TextScenarioTests.Samples.{scenario}.sktest.txt";
        await using Stream stream = assembly.GetManifestResourceStream(resourceName);
        using StreamReader reader = new StreamReader(stream);
        var result = await reader.ReadToEndAsync();
        return result ?? "";
    }

    protected async Task<List<ChatScenario>> LoadChatScenarioAsync(string scenario)
    {
        var testContent = await LoadChatTestAsync(scenario);
        var scenarios = ChatScenarioParser.Parse(testContent, "");
        return scenarios;
    }

    private async Task<string> LoadChatTestAsync(string scenario)
    {
        var assembly = Assembly.GetExecutingAssembly();
        var resourceName = $"skUnit.Tests.SemanticKernel.ChatScenarioTests.Samples.{scenario}.skchat.txt";
        await using Stream stream = assembly.GetManifestResourceStream(resourceName);
        using StreamReader reader = new StreamReader(stream);
        var result = await reader.ReadToEndAsync();
        return result ?? "";
    }


}