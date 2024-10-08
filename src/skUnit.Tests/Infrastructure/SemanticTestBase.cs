﻿using System.Reflection;
using Markdig.Helpers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using skUnit.Scenarios;
using skUnit.Scenarios.Parsers;
using Xunit.Abstractions;

namespace skUnit.Tests.Infrastructure;

public class SemanticTestBase
{
    private readonly string _apiKey;
    private readonly string _endpoint;
    private readonly string _deploymentName;
    protected Kernel Kernel { get; set; }
    protected SemanticKernelAssert SemanticKernelAssert { get; set; }
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

        SemanticKernelAssert = new SemanticKernelAssert(_deploymentName, _endpoint, _apiKey, message => Output.WriteLine(message));
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

    protected async Task<List<InvocationScenario>> LoadInvokeScenarioAsync(string scenario)
    {
        return await InvocationScenario.LoadFromResourceAsync($"skUnit.Tests.ScenarioAssertTests.InvokeScenarioTests.Samples.{scenario}.sktest.md", Assembly.GetExecutingAssembly());
    }

    protected async Task<List<ChatScenario>> LoadChatScenarioAsync(string scenario)
    {
        return await ChatScenario.LoadFromResourceAsync($"skUnit.Tests.ScenarioAssertTests.ChatScenarioTests.Samples.{scenario}.skchat.md", Assembly.GetExecutingAssembly());
    }
}