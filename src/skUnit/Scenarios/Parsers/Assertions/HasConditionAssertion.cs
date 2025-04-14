using System.Collections;
using Microsoft.Extensions.AI;
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
    /// <param name="response"></param>
    /// <param name="history"></param>
    /// <returns></returns>
    public async Task Assert(Semantic semantic, ChatResponse response, IEnumerable<object>? history = null)
    {
        var result = await semantic.HasConditionAsync(response.Text, Condition);

        if (!result.IsValid)
            throw new SemanticAssertException(result.Reason ?? "No reason is provided.");
    }

    public string AssertionType => "Condition";
    public string Description => Condition;

    public override string ToString() => $"{AssertionType}: {Condition}";
}