using System.Text.Json.Nodes;
using System.Text.Json;

namespace skUnit.Scenarios.Parsers.Assertions.FunctionArgumentConditions;

public class ArgumentConditionFactory
{
    public IArgumentCondition Create(JsonNode argument)
    {
        var kind = argument.GetValueKind();
        if (kind != JsonValueKind.Array)
            throw new InvalidOperationException($"""Failed to create argument condition. The specified argument condition format is invalid. Example of valid condition: ["Equals", "value1"].""");

        return CreateArgumentCondition(argument);
    }

    private IArgumentCondition CreateArgumentCondition(JsonNode argument)
    {
        var condition = ArgumentCondition.CreateFromJsonNode(argument);

        switch (condition.Name)
        {
            case Conditions.NotEmpty:
                if (condition.Values.Length > 0)
                    throw new InvalidOperationException("Failed to create argument condition. The `NotEmpty` condition must not have values.");

                return new NotEmptyArgumentCondition();
            case Conditions.Equal:
                if (condition.Values.Length != 1)
                    throw new InvalidOperationException("Failed to create argument condition. The `Equal` condition must have one value.");

                return new EqualsArgumentCondition(condition.Values[0]);
            case Conditions.ContainsAny:
                if (condition.Values.Length == 0)
                    throw new InvalidOperationException("Failed to create argument condition. The `ContainsAny` condition must have at least one value.");

                return new ContainsAnyArgumentCondition(condition.Values);
            case Conditions.IsAnyOf:
                if (condition.Values.Length == 0)
                    throw new InvalidOperationException("Failed to create argument condition. The `IsAnyOf` condition must have at least one value.");

                return new IsAnyOfArgumentCondition(condition.Values);
            case Conditions.SemanticSimilar:
                if (condition.Values.Length != 1)
                    throw new InvalidOperationException("Failed to create argument condition. The `SemanticSimilar` condition must have one value.");

                return new SemanticSimilarArgumentCondition(condition.Values[0]);
            default:
                throw new InvalidOperationException($"Failed to create argument condition. Condition `{condition.Name}` is not supported.");
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
