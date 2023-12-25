using SemanticValidation;
using skUnit.Exceptions;

namespace skUnit.Scenarios.Parsers.Assertions;

public class AreSimilarAssertion : IKernelAssertion
{
    public required string ExpectedAnswer { get; set; }

    public async Task Assert(Semantic semantic, string answer)
    {
        var result = await semantic.AreSimilarAsync(answer, ExpectedAnswer);

        if (!result.IsValid)
            throw new SemanticAssertException(result.Reason ?? "No reason is provided.");
    }

    public string AssertionType => "Similar";
    public string Description => ExpectedAnswer;

    public override string ToString() => $"{AssertionType}: {ExpectedAnswer}";
}