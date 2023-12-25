using Microsoft.SemanticKernel;
using skUnit.Exceptions;
using skUnit.Scenarios;

namespace skUnit;

public partial class SemanticKernelAssert
{
    public static async Task ScenarioSuccessAsync(Kernel kernel, TextScenario scenario)
    {
        Log($"# TEST {scenario.Description}");
        Log("");

        Log($"# PROMPT");
        Log($"{scenario.Prompt}");
        Log("");

        var arguments = new KernelArguments();
        foreach (var parameter in scenario.Parameters)
        {
            arguments.Add(parameter.Key, parameter.Value);
            Log($"## PARAMETER {parameter.Key}");
            Log($"{parameter.Value}");
            Log("");
        }

        Log($"## EXPECTED ANSWER");
        Log(scenario.ExpectedAnswer ?? "");
        Log("");

        var prompt = scenario.Prompt;
        if (string.IsNullOrWhiteSpace(prompt))
        {
            scenario.Parameters.TryGetValue("input", out prompt);
        }

        if (prompt is null)
            throw new InvalidOperationException($"""
                    Prompt is null for scenario: 
                    {scenario.RawText}
                    """);

        var result = await kernel.InvokePromptAsync<string>(prompt, arguments);

        Log($"## ACTUAL ANSWER:");
        Log(result ?? "");
        Log("");

        foreach (var kernelAssert in scenario.Assertions)
        {
            Log($"## CHECK {kernelAssert.AssertionType}");
            Log($"{kernelAssert.Description}");
            await kernelAssert.Assert(Semantic, result);
            Log($"OK");
            Log("");
        }
    }

    public static async Task ScenarioSuccessAsync(Kernel kernel, List<TextScenario> scenarios)
    {
        foreach (var scenario in scenarios)
        {
            await ScenarioSuccessAsync(kernel, scenario);
            Log("");
            Log("----------------------------------");
            Log("");
        }
    }

    public static async Task ScenarioThrowsAsync<TSemanticAssertException>(Kernel kernel, TextScenario scenario) where TSemanticAssertException : SemanticAssertException
    {
        var isThrown = false;
        try
        {
            var arguments = new KernelArguments();
            Log($"# TEST {scenario.Description}");
            Log("");

            Log($"# PROMPT");
            Log($"{scenario.Prompt}");
            Log("");

            foreach (var parameters in scenario.Parameters)
            {
                arguments.Add(parameters.Key, parameters.Value);
                Log($"## PARAMETER {parameters.Key}");
                Log($"{parameters.Value}");
                Log("");
            }

            Log($"## EXPECTED ANSWER");
            Log(scenario.ExpectedAnswer ?? "");
            Log("");

            var prompt = scenario.Prompt;
            if (string.IsNullOrWhiteSpace(prompt))
            {
                scenario.Parameters.TryGetValue("input", out prompt);
            }

            if (prompt is null)
                throw new InvalidOperationException($"""
                    Prompt is null for scenario: 
                    {scenario.RawText}
                    """);

            var result = await kernel.InvokePromptAsync<string>(prompt, arguments);

            Log($"## ACTUAL ANSWER");
            Log(result ?? "");
            Log("");

            foreach (var assertion in scenario.Assertions)
            {
                Log($"## CHECK {assertion.AssertionType}");
                Log($"{assertion.Description}");
                await assertion.Assert(Semantic, result);
                Log($"OK");
                Log("");
            }
        }
        catch (SemanticAssertException exception)
        {
            Log("Exception as EXPECTED:");
            Log(exception.Message);
            Log("");
            isThrown = true;
        }

        if (!isThrown)
        {
            throw new Exception($"Expected for an exception of type: {typeof(TSemanticAssertException).FullName}");
        }
    }

    public static async Task ScenarioThrowsAsync<TSemanticAssertException>(Kernel kernel, List<TextScenario> scenarios) where TSemanticAssertException : SemanticAssertException
    {
        foreach (var scenario in scenarios)
        {
            await ScenarioThrowsAsync<TSemanticAssertException>(kernel, scenario);
            Log("");
            Log("----------------------------------");
            Log("");
        }
    }
}