using SemanticValidation;
using skUnit.Exceptions;

namespace skUnit.Scenarios.Parsers.Assertions;

public class ContainsAnyAssertion : IKernelAssertion
{
    public required string[] Texts { get; set; }

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