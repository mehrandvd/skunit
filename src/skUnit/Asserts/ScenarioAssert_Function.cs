using Microsoft.Extensions.AI;
using Microsoft.SemanticKernel;
using skUnit.Exceptions;
using skUnit.Scenarios;

namespace skUnit;

public partial class ScenarioAssert
{
    /// <summary>
    /// Checks whether the <paramref name="function"/> and <paramref name="kernel"/> can pass the <paramref name="scenario"/>.
    /// </summary>
    /// <remarks>
    /// It runs the scenario using:
    /// <code>function.InvokeAsync</code>
    /// and checks all the assertions specified within the scenario.
    /// </remarks>
    /// <param name="scenario"></param>
    /// <param name="kernel"></param>
    /// <param name="function"></param>
    /// <returns></returns>
    public async Task PassAsync(InvocationScenario scenario, Kernel kernel, KernelFunction function)
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
            await CheckAssertionAsync(assertion, new ChatResponse(new ChatMessage(ChatRole.Assistant, result ?? "")), []);
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
    /// <param name="scenarios"></param>
    /// <param name="kernel"></param>
    /// <param name="function"></param>
    /// <returns></returns>
    public async Task PassAsync(List<InvocationScenario> scenarios, Kernel kernel, KernelFunction function)
    {
        foreach (var scenario in scenarios)
        {
            await PassAsync(scenario, kernel, function);
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
    /// <param name="scenario"></param>
    /// <param name="kernel"></param>
    /// <param name="function"></param>
    /// <returns></returns>
    public async Task ThrowsAsync<TSemanticAssertException>(InvocationScenario scenario, Kernel kernel, KernelFunction function) where TSemanticAssertException : SemanticAssertException
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
                    await assertion.Assert(Semantic, new ChatResponse(new ChatMessage(ChatRole.Assistant, result)), []);
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
    /// <param name="scenarios"></param>
    /// <param name="kernel"></param>
    /// <param name="function"></param>
    /// <returns></returns>
    public async Task ThrowsAsync<TSemanticAssertException>(List<InvocationScenario> scenarios, Kernel kernel, KernelFunction function) where TSemanticAssertException : SemanticAssertException
    {
        foreach (var scenario in scenarios)
        {
            await ThrowsAsync<TSemanticAssertException>(scenario, kernel, function);
            Log("");
            Log("----------------------------------");
            Log("");
        }
    }
}