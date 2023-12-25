using Humanizer;
using skUnit.Scenarios.Parsers;
using skUnit.Scenarios.Parsers.Assertions;

namespace skUnit.Scenarios;

/// <summary>
/// An InvokeScenario is a scenario to test a Kernel, Function or Plugin.
/// It contains the required inputs to call a InvokeAsync like: prompt and parameters.
/// Also it contains the expected output by properties like: ExpectedAnswer and Assertions
/// </summary>
public class InvokeScenario : Scenario<InvokeScenario, InvokeScenarioParser>
{
    /// <summary>
    /// This property will only be used if this scenario is running on a kernel like:
    /// <code>
    /// await kernel.InvokePromptAsync(prompt, arguments)
    /// </code>
    /// So this prompt will NOT affect when testing functions. The functions just will get Parameters as their arguments.
    /// </summary>
    public string? Prompt { get; set; }
    
    public string? Description { get; set; }
    
    public required string RawText { get; set; }
    
    /// <summary>
    /// The parameters that will be passed as arguments while invoking by a kernel or function.
    /// </summary>
    public Dictionary<string, string> Parameters { get; set; } = new();
    
    /// <summary>
    /// All the assertions that should be checked after the result of InvokeAsync is ready.
    /// </summary>
    public List<IKernelAssertion> Assertions { get; set; } = new();
    
    /// <summary>
    /// The ExpectedAnswer that stated as ANSWER in the scenario. It will not be checked directly, unless there is
    /// a specific assertion for it in Assertions.
    /// </summary>
    public string? ExpectedAnswer { get; set; }
}