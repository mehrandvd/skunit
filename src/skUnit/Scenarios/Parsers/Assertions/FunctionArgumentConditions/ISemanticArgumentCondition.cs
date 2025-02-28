using SemanticValidation;

namespace skUnit.Scenarios.Parsers.Assertions.FunctionArgumentConditions;

public interface ISemanticArgumentCondition : IArgumentCondition
{
    Semantic Semantic { get; set; }
}
