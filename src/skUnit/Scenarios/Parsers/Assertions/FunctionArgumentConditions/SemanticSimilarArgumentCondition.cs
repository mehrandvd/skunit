using SemanticValidation;

namespace skUnit.Scenarios.Parsers.Assertions.FunctionArgumentConditions;

public class SemanticSimilarArgumentCondition : ISemanticArgumentCondition
{
    public SemanticSimilarArgumentCondition(string conditionValue)
    {
        ConditionValues = [conditionValue];
    }

    public string Name => Conditions.SemanticSimilar;

    public string[] ConditionValues { get; }

    public Semantic Semantic { get; set; }

    public bool IsMatch(string value)
    {
        var result = Semantic.AreSimilar(value, ConditionValues[0]);
        return result.IsValid;
    }
}
