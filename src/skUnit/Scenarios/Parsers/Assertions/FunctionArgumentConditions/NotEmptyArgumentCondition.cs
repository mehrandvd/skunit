namespace skUnit.Scenarios.Parsers.Assertions.FunctionArgumentConditions;

internal class NotEmptyArgumentCondition : IArgumentCondition
{
    public string Name => Conditions.NotEmpty;

    public bool IsMatch(string value)
    {
        return !string.IsNullOrEmpty(value);
    }
}
