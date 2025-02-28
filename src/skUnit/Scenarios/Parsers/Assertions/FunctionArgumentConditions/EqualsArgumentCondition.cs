namespace skUnit.Scenarios.Parsers.Assertions.FunctionArgumentConditions;

internal class EqualsArgumentCondition : IArgumentCondition
{
    private string conditionValue;

    public EqualsArgumentCondition(string conditionValue)
    {
        this.conditionValue = conditionValue;
    }

    public string Name => Conditions.Equals;

    public bool IsMatch(string value)
        => conditionValue == value;
}
