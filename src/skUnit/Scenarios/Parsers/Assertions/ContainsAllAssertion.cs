using System.Collections;
using Microsoft.Extensions.AI;
using skUnit.Exceptions;
using skUnit.Runners;

namespace skUnit.Scenarios.Parsers.Assertions;

/// <summary>
/// Checks if the input contains all of Texts
/// </summary>
public class ContainsAllAssertion : IChatAssertion
{
    /// <summary>
    /// The texts that should be available within the input.
    /// </summary>
    public required string[] Texts { get; set; }

    /// <summary>
    /// Checks if the <paramref name="input"/> contains all strings in Texts/>.
    /// </summary>
    /// <param name="semantic"></param>
    /// <param name="response"></param>
    /// <param name="history"></param>
    /// <returns></returns>
    /// <exception cref="SemanticAssertException"></exception>
    public Task Assert(SemanticAgent semantic, ChatResponse response, IList<ChatMessage>? history = null)
    {
        var notFounds = Texts.Where(t => !response.Text.Contains(t.Trim())).ToList();

        if (notFounds.Any())
            throw new SemanticAssertException($"Text does not contain these: '{string.Join(", ", notFounds)}'");

        return Task.CompletedTask;
    }

    public string AssertionType => "ContainsAll";
    public string Description => string.Join(", ", Texts);

    public override string ToString() => $"{AssertionType}: {Texts}";
}