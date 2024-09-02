using Microsoft.SemanticKernel;
using skUnit.Exceptions;
using skUnit.Scenarios;

namespace skUnit;

public partial class SemanticKernelAssert
{
    /// <summary>
    /// Checks whether the <paramref name="function"/> and <paramref name="kernel"/> can pass the <paramref name="scenario"/>.
    /// </summary>
    /// <remarks>
    /// It runs the scenario using:
    /// <code>function.InvokeAsync</code>
    /// and checks all the assertions specified within the scenario.
    /// </remarks>
    /// <param name="kernel"></param>
    /// <param name="function"></param>
    /// <param name="scenario"></param>
    /// <returns></returns>
    public async Task CheckScenarioAsync(Kernel kernel, KernelFunction function, InvocationScenario scenario)
    {
        var arguments = new KernelArguments();
        Log($"# SCENARIO {scenario.Description}");
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

        foreach (var assertion in scenario.Assertions)
        {
            await CheckAssertionAsync(assertion, result ?? "");
        }
    }

    /// <summary>
    /// Checks whether the <paramref name="function"/> and <paramref name="kernel"/> can pass all the <paramref name="scenarios"/>.
    /// </summary>
    /// <remarks>
    /// It runs the scenario using:
    /// <code>function.InvokeAsync</code>
    /// and checks all the assertions specified within the scenario.
    /// </remarks>
    /// <param name="kernel"></param>
    /// <param name="function"></param>
    /// <param name="scenarios"></param>
    /// <returns></returns>
    public async Task CheckScenarioAsync(
        Kernel kernel, 
        KernelFunction function,
        List<InvocationScenario> scenarios)
    {
        foreach (var scenario in scenarios)
        {
            await CheckScenarioAsync(kernel, function, scenario);
            Log("");
            Log("----------------------------------");
            Log("");
        }
    }

    /// <summary>
    /// Checks whether the <paramref name="function"/> and <paramref name="kernel"/>
    /// throws <typeparamref name="TSemanticAssertException"/> while trying to pass the <paramref name="scenario"/>.
    /// </summary>
    /// <remarks>
    /// It runs the scenario using:
    /// <code>function.InvokeAsync</code>
    /// and checks all the assertions specified within the scenario.
    /// </remarks>
    /// <param name="kernel"></param>
    /// <param name="function"></param>
    /// <param name="scenario"></param>
    /// <returns></returns>
    public async Task ScenarioThrowsAsync<TSemanticAssertException>(
        Kernel kernel, 
        KernelFunction function,
        InvocationScenario scenario
        ) where TSemanticAssertException : SemanticAssertException
    {
        var isThrown = false;
        try
        {
            var arguments = new KernelArguments();
            Log($"# SCENARIO {scenario.Description}");
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

                try
                {
                    await assertion.Assert(Semantic, result);
                    Log($"✅ OK");
                    Log("");
                }
                catch (SemanticAssertException exception)
                {
                    Log("❌ FAIL");
                    Log("Reason:");
                    Log(exception.Message);
                    Log();
                    throw;
                }
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

    /// <summary>
    /// Checks whether the <paramref name="function"/> and <paramref name="kernel"/>
    /// throws <typeparamref name="TSemanticAssertException"/> while trying to pass each of the <paramref name="scenarios"/>.
    /// </summary>
    /// <remarks>
    /// It runs the scenario using:
    /// <code>function.InvokeAsync</code>
    /// and checks all the assertions specified within the scenario.
    /// </remarks>
    /// <param name="kernel"></param>
    /// <param name="function"></param>
    /// <param name="scenarios"></param>
    /// <returns></returns>
    public async Task ScenarioThrowsAsync<TSemanticAssertException>(
        Kernel kernel, 
        KernelFunction function,
        List<InvocationScenario> scenarios
        ) where TSemanticAssertException : SemanticAssertException
    {
        foreach (var scenario in scenarios)
        {
            await ScenarioThrowsAsync<TSemanticAssertException>(kernel, function, scenario);
            Log("");
            Log("----------------------------------");
            Log("");
        }
    }

    /// <summary>
    /// Checks whether the <paramref name="pluginUrl"/> can pass the <paramref name="scenario"/>.
    /// </summary>
    /// <remarks>
    /// It runs the scenario using:
    /// <code>kernel.InvokeAsync</code>
    /// and checks all the assertions specified within the scenario.
    /// </remarks>
    /// <param name="pluginUrl"></param>
    /// <param name="scenario"></param>
    /// <returns></returns>
    public async Task CheckPluginByUrlAsync(string pluginUrl, InvocationScenario scenario)
    {
        var kernel = new KernelBuilder().WithOpenAIChatCompletionService(pluginUrl).Build();
        var function = kernel.GetFunction("pluginFunctionName"); // Replace with actual function name
        await CheckScenarioAsync(kernel, function, scenario);
    }

    /// <summary>
    /// Checks whether the <paramref name="pluginUrl"/> can pass all the <paramref name="scenarios"/>.
    /// </summary>
    /// <remarks>
    /// It runs the scenario using:
    /// <code>kernel.InvokeAsync</code>
    /// and checks all the assertions specified within the scenario.
    /// </remarks>
    /// <param name="pluginUrl"></param>
    /// <param name="scenarios"></param>
    /// <returns></returns>
    public async Task CheckPluginByUrlAsync(string pluginUrl, List<InvocationScenario> scenarios)
    {
        var kernel = new KernelBuilder().WithOpenAIChatCompletionService(pluginUrl).Build();
        var function = kernel.GetFunction("pluginFunctionName"); // Replace with actual function name
        await CheckScenarioAsync(kernel, function, scenarios);
    }
}
