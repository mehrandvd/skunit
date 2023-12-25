using SemanticValidation;
using skUnit.Exceptions;

namespace skUnit.Scenarios.Parsers.Assertions;

public class ContainsAllAssertion : IKernelAssertion
{
    public required string[] Texts { get; set; }

    public async Task Assert(Semantic semantic, string answer)
    {
        var notFounds = Texts.Where(t=>!answer.Contains(t.Trim())).ToList();

        if (notFounds.Any())
            throw new SemanticAssertException($"Text does not contain these: '{string.Join(", ", notFounds)}'");
    }

    public string AssertionType => "ContainsAll";
    public string Description => string.Join(", ",Texts);

    public override string ToString() => $"{AssertionType}: {Texts}";
}