namespace skUnit.Scenarios.Parsers.Assertions.FunctionArgumentConditions;

public class ContainsAnyArgumentCondition : IArgumentCondition
{
    public ContainsAnyArgumentCondition(string[] conditionValues)
    {
        ConditionValues = conditionValues;
    }

    public string Name => Conditions.ContainsAny;

    public string[] ConditionValues { get; }

    public bool IsMatch(string value)
    {
        return ConditionValues.Any(value.Contains);
    }
}
