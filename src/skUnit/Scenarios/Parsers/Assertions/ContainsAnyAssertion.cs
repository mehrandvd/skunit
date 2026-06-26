using Microsoft.Extensions.AI;
using skUnit.Exceptions;
using skUnit.Runners;

namespace skUnit.Scenarios.Parsers.Assertions;

/// <summary>
/// Checks if the input contains any of Texts
/// </summary>
public class ContainsAnyAssertion : IChatAssertion
{
    /// <summary>
    /// The texts that should be available within the input.
    /// </summary>
    public required string[] Texts { get; set; }

    /// <summary>
    /// Checks if the <paramref name="input"/> contains any of strings in Texts/>.
    /// </summary>
    /// <param name="semantic"></param>
    /// <param name="response"></param>
    /// <param name="history"></param>
    /// <returns></returns>
    /// <exception cref="SemanticAssertException"></exception>
    public Task Assert(SemanticEvaluator semanticEvaluator, ChatResponse response, IReadOnlyList<ChatMessage>? history = null, CancellationToken cancellationToken = default)
    {
        var founds = Texts.Where(t => response.Text.Contains(t.Trim())).ToList();

        if (!founds.Any())
            throw new SemanticAssertException($"Text does not contain any of these: '{string.Join(", ", Texts)}'");

        return Task.CompletedTask;
    }

    public string AssertionType => "ContainsAny";
    public string Description => string.Join(", ", Texts);

    public override string ToString() => $"{AssertionType}: {Texts}";
}