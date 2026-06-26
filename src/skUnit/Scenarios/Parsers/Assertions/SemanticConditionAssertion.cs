using System.Collections;
using Microsoft.Extensions.AI;
using skUnit.Exceptions;
using skUnit.Runners;

namespace skUnit.Scenarios.Parsers.Assertions;

/// <summary>
/// Checks whether the input has the condition semantically.
/// </summary>
public class SemanticConditionAssertion : IChatAssertion
{
    public required string Condition { get; set; }

    /// <summary>
    /// Checks whether the <paramref name="input"/> has the Condition semantically using <paramref name="semantic"/>.
    /// </summary>
    /// <param name="semantic"></param>
    /// <param name="response"></param>
    /// <param name="history"></param>
    /// <returns></returns>
    public async Task Assert(SemanticEvaluator semanticEvaluator, ChatResponse response, IReadOnlyList<ChatMessage>? history = null, CancellationToken cancellationToken = default)
    {
        var result = await semanticEvaluator.HasConditionAsync(response.Text, Condition, cancellationToken);

        if (!result.IsValid)
            throw new SemanticAssertException(result.Reason ?? "No reason is provided.");
    }

    public string AssertionType => "SemanticCondition";
    public string Description => Condition;

    public override string ToString() => $"{AssertionType}: {Condition}";
}