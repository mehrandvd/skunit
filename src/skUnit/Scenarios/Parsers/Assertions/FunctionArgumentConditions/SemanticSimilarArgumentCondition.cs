using SemanticValidation;

namespace skUnit.Scenarios.Parsers.Assertions.FunctionArgumentConditions;

public class SemanticSimilarArgumentCondition : ISemanticArgumentCondition
{
    private readonly string conditionValue;

    public SemanticSimilarArgumentCondition(string conditionValue)
    {
        this.conditionValue = conditionValue;
    }

    public string Name => Conditions.SemanticSimilar;

    public Semantic Semantic { get; set; }

    public bool IsMatch(string value)
    {
        var result = Semantic.AreSimilar(value, conditionValue);
        return result.IsValid;
    }
}
