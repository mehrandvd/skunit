namespace skUnit.Scenarios.Parsers.Assertions.FunctionArgumentConditions;

public interface IArgumentCondition
{
    string Name { get; }

    string[] ConditionValues { get; }

    bool IsMatch(string value);
}
