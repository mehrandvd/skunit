namespace skUnit.Scenarios.Parsers.Assertions.FunctionArgumentConditions;

public class EqualsArgumentCondition : IArgumentCondition
{
    public EqualsArgumentCondition(string conditionValue)
    {
        ConditionValues = [conditionValue];
    }

    public string Name => Conditions.Equal;

    public string[] ConditionValues { get; }

    public bool IsMatch(string value)
        => ConditionValues[0] == value;
}
