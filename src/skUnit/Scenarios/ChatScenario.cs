using skUnit.Parsers.Assertions;

namespace skUnit.Scenarios;

public class ChatScenario : Scenario
{
    public string? Description { get; set; }
    public required string RawText { get; set; }
    public string? ExpectedAnswer { get; set; }
    public Dictionary<string, string> Arguments { get; set; } = new();
    public List<IKernelAssertion> Asserts { get; set; } = new();

}