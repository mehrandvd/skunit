namespace skUnit.Scenarios.Parsers.Assertions.FunctionArgumentConditions;

public class IsAnyOfArgumentCondition : IArgumentCondition
{
    public IsAnyOfArgumentCondition(string[] conditionValues)
    {
        ConditionValues = conditionValues;
    }

    public string Name => Conditions.IsAnyOf;

    public string[] ConditionValues { get; }

    public bool IsMatch(string value)
    {
        return ConditionValues.Any(target => target == value);
    }
}
