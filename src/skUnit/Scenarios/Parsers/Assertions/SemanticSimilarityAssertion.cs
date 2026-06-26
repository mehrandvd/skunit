using System.Collections;
using Microsoft.Extensions.AI;
using skUnit.Exceptions;
using skUnit.Runners;

namespace skUnit.Scenarios.Parsers.Assertions;

/// <summary>
/// Checks if the answer is similar to ExpectedAnswer
/// </summary>
public class SemanticSimilarityAssertion : IChatAssertion
{
    /// <summary>
    /// The expected answer that the actual answer should compared with.
    /// </summary>
    public required string ExpectedAnswer { get; set; }

    /// <summary>
    /// Checks if <paramref name="answer"/> is similar to ExpectedAnswer using <paramref name="semantic"/>
    /// </summary>
    /// <param name="semantic"></param>
    /// <param name="response"></param>
    /// <param name="history"></param>
    /// <returns></returns>
    /// <exception cref="SemanticAssertException"></exception>
    public async Task Assert(SemanticEvaluator semanticEvaluator, ChatResponse response, IReadOnlyList<ChatMessage>? history = null, CancellationToken cancellationToken = default)
    {
        var result = await semanticEvaluator.AreSimilarAsync(response.Text, ExpectedAnswer, cancellationToken);

        if (!result.IsValid)
            throw new SemanticAssertException(result.Reason ?? "No reason is provided.");
    }

    public string AssertionType => "SemanticSimilar";
    public string Description => ExpectedAnswer;

    public override string ToString() => $"{AssertionType}: {ExpectedAnswer}";
}