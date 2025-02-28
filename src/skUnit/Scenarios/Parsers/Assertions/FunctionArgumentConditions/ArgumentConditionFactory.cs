using System.Text.Json.Nodes;
using System.Text.Json;

namespace skUnit.Scenarios.Parsers.Assertions.FunctionArgumentConditions;

internal class ArgumentConditionFactory
{
    public IArgumentCondition Create(JsonNode argument)
    {
        var kind = argument.GetValueKind();
        if (kind != JsonValueKind.Array)
            throw new InvalidOperationException($"""Failed to create argument condition. The specified argument condtion format is invalid. Example of valid condition: ["Equals", "value1"].""");

        return CreateArgumentCondition(argument);
    }

    private IArgumentCondition CreateArgumentCondition(JsonNode argument)
    {
        var condition = ArgumentCondition.CreateFromJsonNode(argument);

        switch (condition.Name)
        {
            case Conditions.Equals:
                return new EqualsArgumentCondition(condition.Values[0]);
            case Conditions.ContainsAny:
                return new ContainsAnyArgumentCondition(condition.Values);
            case Conditions.IsAnyOf:
                return new IsAnyOfArgumentCondition(condition.Values);
            case Conditions.NotEmpty:
                return new NotEmptyArgumentCondition();
            case Conditions.SemanticSimilar:
                return new SemanticSimilarArgumentCondition(condition.Values[0]);
            default:
                throw new InvalidOperationException($"Failed to create argument condition. Condition {condition.Name} is not supported.");
        }
    }

    private class ArgumentCondition
    {
        public string Name { get; set; }

        public string[] Values { get; set; }

        public static ArgumentCondition CreateFromJsonNode(JsonNode argument)
        {
            var condition = argument.AsArray();

            if (condition[0] is JsonValue conditionName && conditionName.TryGetValue<string>(out var name))
            {
                var values = condition
                    .Skip(1)
                    .Where(value => value is JsonValue)
                    .Select(value => value.GetValue<string>())
                    .ToArray();

                return new()
                {
                    Name = name,
                    Values = values,
                };
            }

            throw new InvalidOperationException($"""Failed to create argument condition. The specified argument condtion format is invalid. Example of valid condition: ["Equals", "value1"].""");
        }
    }
}
