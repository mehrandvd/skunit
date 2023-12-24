using SemanticValidation;

namespace skUnit.Scenarios.Parsers.Assertions;

public class HasConditionAssertion : IKernelAssertion
{
    public required string Condition { get; set; }

    public async Task Assert(Semantic semantic, string answer)
    {
        await semantic.HasConditionAsync(answer, Condition);
    }

    public string AssertionType => "Condition";
    public string Description => Condition;

    public override string ToString() => $"{AssertionType}: {Condition}";
}