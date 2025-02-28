namespace skUnit.Scenarios.Parsers.Assertions.FunctionArgumentConditions;

internal class ContainsAnyArgumentCondition : IArgumentCondition
{
    private string[] conditionValues;

    public ContainsAnyArgumentCondition(string[] conditionValues)
    {
        this.conditionValues = conditionValues;
    }

    public string Name => Conditions.ContainsAny;

    public bool IsMatch(string value)
    {
        return conditionValues.Any(value.Contains);
    }
}
