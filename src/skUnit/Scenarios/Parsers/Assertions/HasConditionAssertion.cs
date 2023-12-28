using SemanticValidation;
using skUnit.Exceptions;

namespace skUnit.Scenarios.Parsers.Assertions;

/// <summary>
/// Checks whether the input has the condition semantically.
/// </summary>
public class HasConditionAssertion : IKernelAssertion
{
    public required string Condition { get; set; }

    /// <summary>
    /// Checks whether the <paramref name="input"/> has the Condition semantically using <paramref name="semantic"/>.
    /// </summary>
    /// <param name="semantic"></param>
    /// <param name="input"></param>
    /// <returns></returns>
    public async Task Assert(Semantic semantic, string input)
    {
        var result = await semantic.HasConditionAsync(input, Condition);

        if (!result.IsValid)
            throw new SemanticAssertException(result.Reason ?? "No reason is provided.");
    }

    public string AssertionType => "Condition";
    public string Description => Condition;

    public override string ToString() => $"{AssertionType}: {Condition}";
}