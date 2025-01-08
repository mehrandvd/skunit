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
    /// <param name="input"></param>
    /// <param name="historytory"></param>
    /// <param name="answer"></param>
    /// <returns></returns>
    /// <exception cref="SemanticAssertException"></exception>
    public async Task Assert(Semantic semantic, string input, IEnumerable<object>? history = null)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            throw new SemanticAssertException($"""
                                    Expected not empty, but empty:
                                    {input} 
                                    """);
        }
    }

    public string AssertionType => "NotEmpty";
    public string Description => "NotEmpty";

    public override string ToString() => $"{AssertionType}";
}