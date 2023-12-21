using Microsoft.SemanticKernel;
using System.Text.Json;
using SemanticValidation.Models;

namespace SemanticValidation;

public static partial class Semantic
{
    private static Kernel? _testKernel;
    private static Kernel TestKernel
    {
        get => _testKernel ?? throw new InvalidOperationException("SemanticAssert is not initialized yet.");
        set => _testKernel = value;
    }

    private static KernelFunction? _areSameFunc;
    public static KernelFunction AreSameSkFunc
    {
        get => _areSameFunc ?? throw new InvalidOperationException("SemanticAssert is not initialized yet.");
        set => _areSameFunc = value;
    }

    private static KernelFunction? _hasConditionFunc;
    public static KernelFunction HasConditionFunc
    {
        get => _hasConditionFunc ?? throw new InvalidOperationException("SemanticAssert is not initialized yet.");
        set => _hasConditionFunc = value;
    }

    public static void Initialize(string endpoint, string apiKey)
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