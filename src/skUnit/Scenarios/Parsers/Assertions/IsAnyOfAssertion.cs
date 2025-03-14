using SemanticValidation;
using skUnit.Exceptions;

namespace skUnit.Scenarios.Parsers.Assertions;

/// <summary>
/// Checks if the input equals to any of Texts
/// </summary>
public class IsAnyOfAssertion : IKernelAssertion
{
    /// <summary>
    /// The texts that should be available within the input.
    /// </summary>
    public required string[] Texts { get; set; }

    public string AssertionType => "IsAnyOf";

    public string Description => string.Join(",", Texts);

    /// <summary>
    /// Checks if the <paramref name="input"/> equals to any of strings in Texts/>.
    /// </summary>
    /// <param name="semantic"></param>
    /// <param name="input"></param>
    /// <param name="historytory"></param>
    /// <returns></returns>
    /// <exception cref="SemanticAssertException"></exception>
    public Task Assert(Semantic semantic, string input, IEnumerable<object> history = null)
    {
        if (Texts.Any(text => text == input))
            return Task.CompletedTask;

        throw new SemanticAssertException($"Text is not equal to any of these: '{string.Join(",", Texts)}'");
    }

    public override string ToString() => $"{AssertionType}: {Texts}";
}
