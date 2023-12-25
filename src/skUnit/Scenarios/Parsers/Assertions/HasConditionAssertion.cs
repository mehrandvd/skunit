using SemanticValidation;

namespace skUnit.Scenarios.Parsers.Assertions;

/// <summary>
/// Checks whether the answer has the condition semantically.
/// </summary>
public class HasConditionAssertion : IKernelAssertion
{
    public required string Condition { get; set; }

    /// <summary>
    /// Checks whether the <paramref name="answer"/> has the Condition semantically using <paramref name="semantic"/>.
    /// </summary>
    /// <param name="semantic"></param>
    /// <param name="answer"></param>
    /// <returns></returns>
    public async Task Assert(Semantic semantic, string answer)
    {
        await semantic.HasConditionAsync(answer, Condition);
    }

    public string AssertionType => "Condition";
    public string Description => Condition;

    public override string ToString() => $"{AssertionType}: {Condition}";
}