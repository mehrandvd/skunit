namespace skUnit.Scenarios.Parsers.Assertions.FunctionArgumentConditions;

internal class IsAnyOfArgumentCondition : IArgumentCondition
{
    private string[] conditionValues;

    public IsAnyOfArgumentCondition(string[] conditionValues)
    {
        this.conditionValues = conditionValues;
    }

    public string Name => Conditions.IsAnyOf;
    
    public bool IsMatch(string value)
    {
        return conditionValues.Any(target => target == value);
    }
}
