using System.Collections;
using Microsoft.Extensions.AI;
using SemanticValidation;
using skUnit.Exceptions;

namespace skUnit.Scenarios.Parsers.Assertions;

/// <summary>
/// Checks if the answer is not empty
/// </summary>
public class NotEmptyAssertion : IKernelAssertion
{
    /// <summary>
    /// Checks if the <paramref name="answer"/> is not empty/>.
    /// </summary>
    /// <param name="semantic"></param>
    /// <param name="response"></param>
    /// <param name="historytory"></param>
    /// <param name="answer"></param>
    /// <returns></returns>
    /// <exception cref="SemanticAssertException"></exception>
    public Task Assert(Semantic semantic, ChatResponse response, IList<ChatMessage>? history = null)
    {
        if (string.IsNullOrWhiteSpace(response.Text))
        {
            throw new SemanticAssertException($"""
                                    Expected not empty, but empty:
                                    {response.Text} 
                                    """);
        }

        return Task.CompletedTask;
    }

    public string AssertionType => "NotEmpty";
    public string Description => "NotEmpty";

    public override string ToString() => $"{AssertionType}";
}