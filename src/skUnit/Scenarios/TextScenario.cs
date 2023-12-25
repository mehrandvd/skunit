using skUnit.Scenarios.Parsers.Assertions;

namespace skUnit.Scenarios;

/// <summary>
/// An InvokeScenario is a scenario to test a Kernel, Function or Plugin.
/// It contains the required inputs to call a InvokeAsync like: prompt and parameters.
/// Also it contains the expected output by properties like: ExpectedAnswer and Assertions
/// </summary>
public class InvokeScenario
{
    public string? Prompt { get; set; }
    public string? Description { get; set; }
    public required string RawText { get; set; }
    public Dictionary<string, string> Parameters { get; set; } = new();
    public List<IKernelAssertion> Assertions { get; set; } = new();
    public string? ExpectedAnswer { get; set; }
}