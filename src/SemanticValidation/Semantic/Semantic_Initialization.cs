using Microsoft.SemanticKernel;
using System.Text.Json;
using SemanticValidation.Models;

namespace SemanticValidation;

public partial class Semantic
{
    private Kernel TestKernel { get; }

    public KernelFunction AreSameSkFunc { get; set; }

    public KernelFunction HasConditionFunc { get; set; }

    public Semantic(string endpoint, string apiKey)
    {
        var builder = Kernel.CreateBuilder();
        builder.AddAzureOpenAIChatCompletion("gpt-35-turbo-test", endpoint, apiKey);

        TestKernel = builder.Build();

        var dir = Environment.CurrentDirectory;

        var testPlugin = TestKernel.CreatePluginFromPromptDirectory(Path.Combine(dir, "Plugins", "TestPlugin"));

        AreSameSkFunc = testPlugin["AreSame"];
        HasConditionFunc = testPlugin["HasCondition"];

    }
}