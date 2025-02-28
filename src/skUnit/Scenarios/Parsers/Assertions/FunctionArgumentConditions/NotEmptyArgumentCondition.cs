namespace skUnit.Scenarios.Parsers.Assertions.FunctionArgumentConditions;

public class NotEmptyArgumentCondition : IArgumentCondition
{
    public string Name => Conditions.NotEmpty;

    public string[] ConditionValues => [];

    public bool IsMatch(string value)
    {
        return !string.IsNullOrEmpty(value);
    }
}
