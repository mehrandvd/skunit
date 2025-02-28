namespace skUnit.Scenarios.Parsers.Assertions.FunctionArgumentConditions;

public interface IArgumentCondition
{
    string Name { get; }

    bool IsMatch(string value);
}
