using Microsoft.SemanticKernel;
using skUnit.Exceptions;
using skUnit.Scenarios;

namespace skUnit;

public partial class SemanticKernelAssert
{
    public static async Task ScenarioSuccessAsync(Kernel kernel, KernelFunction function, TextScenario scenario)
    {
        var arguments = new KernelArguments();
        Log($"# TEST {scenario.Description}");
        Log("");
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

        var result = await function.InvokeAsync<string>(kernel, arguments);

        Log($"## ACTUAL ANSWER");
        Log(result ?? "");
        Log("");

        foreach (var kernelAssert in scenario.Assertions)
        {
            Log($"## ANSWER {kernelAssert.AssertionType}");
            Log($"{kernelAssert.Description}");
            await kernelAssert.Assert(Semantic, result);
            Log($"OK");
            Log("");
        }
    }

    public static async Task ScenarioSuccessAsync(Kernel kernel, KernelFunction function,
        List<TextScenario> scenarios)
    {
        foreach (var scenario in scenarios)
        {
            await ScenarioSuccessAsync(kernel, function, scenario);
            Log("");
            Log("----------------------------------");
            Log("");
        }
    }

    public static async Task ScenarioThrowsAsync<TSemanticAssertException>(Kernel kernel, KernelFunction function,
        TextScenario scenario) where TSemanticAssertException : SemanticAssertException
    {
        var isThrown = false;
        try
        {
            var arguments = new KernelArguments();
            Log($"# TEST {scenario.Description}");
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

            var result = await function.InvokeAsync<string>(kernel, arguments);

            Log($"## ACTUAL ANSWER");
            Log(result ?? "");
            Log("");

            foreach (var assertion in scenario.Assertions)
            {
                Log($"## ANSWER {assertion.AssertionType}");
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

    public static async Task ScenarioThrowsAsync<TSemanticAssertException>(Kernel kernel, KernelFunction function,
        List<TextScenario> scenarios) where TSemanticAssertException : SemanticAssertException
    {
        foreach (var scenario in scenarios)
        {
            await ScenarioThrowsAsync<TSemanticAssertException>(kernel, function, scenario);
            Log("");
            Log("----------------------------------");
            Log("");
        }
    }
}