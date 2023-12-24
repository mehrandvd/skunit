using SemanticValidation;
using skUnit.Exceptions;

namespace skUnit.Scenarios.Parsers.Assertions;

public class AreSameAssertion : IKernelAssertion
{
    public required string ExpectedAnswer { get; set; }

    public async Task Assert(Semantic semantic, string answer)
    {
        var result = await semantic.AreSameAsync(answer, ExpectedAnswer);

        if (!result.Success)
            throw new SemanticAssertException(result.Message);
    }

    public string AssertionType => "Same";
    public string Description => ExpectedAnswer;

    public override string ToString() => $"{AssertionType}: {ExpectedAnswer}";
}