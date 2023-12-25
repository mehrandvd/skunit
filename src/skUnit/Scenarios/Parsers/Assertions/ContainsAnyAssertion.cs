using SemanticValidation;
using skUnit.Exceptions;

namespace skUnit.Scenarios.Parsers.Assertions;

/// <summary>
/// Checks if the answer contains any of Texts
/// </summary>
public class ContainsAnyAssertion : IKernelAssertion
{
    /// <summary>
    /// The texts that should be available within the answer.
    /// </summary>
    public required string[] Texts { get; set; }

    /// <summary>
    /// Checks if the <paramref name="answer"/> contains any of strings in Texts/>.
    /// </summary>
    /// <param name="semantic"></param>
    /// <param name="answer"></param>
    /// <returns></returns>
    /// <exception cref="SemanticAssertException"></exception>
    public async Task Assert(Semantic semantic, string answer)
    {
        var founds = Texts.Where(t => answer.Contains(t.Trim())).ToList();

        if (!founds.Any())
            throw new SemanticAssertException($"Text does not contain any of these: '{string.Join(", ", Texts)}'");
    }

    public string AssertionType => "ContainsAny";
    public string Description => string.Join(", ", Texts);

    public override string ToString() => $"{AssertionType}: {Texts}";
}