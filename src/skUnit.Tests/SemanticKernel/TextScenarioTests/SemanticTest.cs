using System.Reflection;
using Microsoft.SemanticKernel;
using skUnit.Parsers;
using skUnit.Scenarios;
using Xunit.Abstractions;

namespace skUnit.Tests.SemanticKernel.TextScenarioTests;

public class SemanticTest
{
    protected string _apiKey;
    protected string _endpoint;
    protected Kernel Kernel { get; set; }
    protected ITestOutputHelper Output { get; set; }

    public SemanticTest(ITestOutputHelper output)
    {
        Output = output;
        _apiKey = Environment.GetEnvironmentVariable("openai-api-key", EnvironmentVariableTarget.User) ??
                  throw new Exception("No ApiKey in environment variables.");
        _endpoint = Environment.GetEnvironmentVariable("openai-endpoint", EnvironmentVariableTarget.User) ??
                    throw new Exception("No Endpoint in environment variables.");

        SemanticKernelAssert.Initialize(_endpoint, _apiKey, message => Output.WriteLine(message));
        Kernel = CreateKernel();
    }

    Kernel CreateKernel()
    {
        var builder = Kernel.CreateBuilder();
        builder.AddAzureOpenAIChatCompletion("gpt-35-turbo-test", _endpoint, _apiKey);

        var kernel = builder.Build();
        return kernel;
    }

    protected async Task<List<TextScenario>> LoadScenarioAsync(string scenario)
    {
        var testContent = await LoadTextTestAsync(scenario);
        var scenarios = TextScenarioParser.Parse(testContent, "");
        return scenarios;
    }
    private async Task<string> LoadTextTestAsync(string scenario)
    {
        var assembly = Assembly.GetExecutingAssembly();
        var resourceName = $"skUnit.Tests.SemanticKernel.{scenario}.sktest.txt";
        await using Stream stream = assembly.GetManifestResourceStream(resourceName);
        using StreamReader reader = new StreamReader(stream);
        var result = await reader.ReadToEndAsync();
        return result ?? "";
    }


}