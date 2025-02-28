using SemanticValidation;

namespace skUnit.Scenarios.Parsers.Assertions.FunctionArgumentConditions;

internal interface ISemanticArgumentCondition : IArgumentCondition
{
    Semantic Semantic { get; set; }
}
