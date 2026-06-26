using skUnit.Exceptions;
using Microsoft.Extensions.AI;
using FunctionCallContent = Microsoft.Extensions.AI.FunctionCallContent;
using FunctionResultContent = Microsoft.Extensions.AI.FunctionResultContent;
using System.Text.Json.Nodes;
using skUnit.Utils;
using skUnit.Runners;

namespace skUnit.Scenarios.Parsers.Assertions
{
    public enum FunctionCallMatchStrategy
    {
        AnyCall,
        LastCall,
        ExactSingleCall
    }

    public sealed class FunctionCallAssertionSpec
    {
        public required string FunctionName { get; init; }
        public Dictionary<string, IChatAssertion> ArgumentAssertions { get; init; } = [];
        public FunctionCallMatchStrategy MatchStrategy { get; init; } = FunctionCallMatchStrategy.LastCall;
    }

    public class FunctionCallAssertion : IChatAssertion
    {
        private string RawSpec { get; set; } = default!;

        public JsonObject? FunctionCallJson { get; private set; }

        public FunctionCallAssertionSpec? Spec { get; private set; }

        public Dictionary<string, IChatAssertion> FunctionArguments => Spec?.ArgumentAssertions ?? [];

        public FunctionCallAssertion ParseSpec(string jsonAssertText)
        {
            if (string.IsNullOrWhiteSpace(jsonAssertText))
                throw new InvalidOperationException("The FunctionCallCheck is empty.");

            RawSpec = jsonAssertText;
            FunctionCallJson = null;
            Spec = null;

            var trimmedSpec = RawSpec.Trim();
            var looksLikeJson = trimmedSpec.StartsWith('{')
                || trimmedSpec.StartsWith('[')
                || trimmedSpec.StartsWith("```", StringComparison.Ordinal);

            if (looksLikeJson)
            {
                var json = SemanticUtils.PowerParseJson<JsonObject>(RawSpec);
                FunctionCallJson = json;
            }

            if (FunctionCallJson is null)
            {
                Spec = new FunctionCallAssertionSpec
                {
                    FunctionName = RawSpec.Trim()
                };
                return this;
            }

            var functionName = FunctionCallJson["function_name"]?.GetValue<string>()?.Trim();
            if (string.IsNullOrWhiteSpace(functionName))
            {
                throw new InvalidOperationException("function_name is required for FunctionCall assertion.");
            }

            var argumentAssertions = new Dictionary<string, IChatAssertion>(StringComparer.OrdinalIgnoreCase);
            if (FunctionCallJson["arguments"] is JsonObject argumentsJson)
            {
                foreach (var kv in argumentsJson)
                {
                    string checkValuesText;
                    string checkType;

                    if (kv.Value is JsonValue checkValue)
                    {
                        checkType = "Equals";
                        checkValuesText = checkValue.GetValue<string>();
                    }
                    else if (kv.Value is JsonArray checkArray)
                    {
                        checkType = checkArray[0]?.GetValue<string>() ?? throw new InvalidOperationException("No valid array assertion.");
                        var checkValues = checkArray
                            .Skip(1)
                            .Where(value => value is not null)
                            .Select(value => value?.GetValue<string>());
                        checkValuesText = string.Join(", ", checkValues);
                    }
                    else
                    {
                        throw new InvalidOperationException(
                            $"""
                            JsonCheck has not a proper value supported json types are string and array:
                            {kv}
                            """);
                    }

                    argumentAssertions[kv.Key] = ChatAssertionParser.Parse(checkValuesText, checkType);
                }
            }

            var matchStrategy = FunctionCallMatchStrategy.LastCall;
            if (FunctionCallJson["match_strategy"] is JsonValue strategyValue)
            {
                var strategyText = strategyValue.GetValue<string>();
                if (!Enum.TryParse(strategyText, ignoreCase: true, out matchStrategy))
                {
                    throw new InvalidOperationException($"Invalid function call match strategy: {strategyText}");
                }
            }

            Spec = new FunctionCallAssertionSpec
            {
                FunctionName = functionName,
                ArgumentAssertions = argumentAssertions,
                MatchStrategy = matchStrategy,
            };

            return this;
        }

        /// <summary>
        /// Checks if <paramref name="answer"/> is meets the conditions in FunctionCallJson <paramref name="semantic"/>
        /// </summary>
        /// <param name="semanticEvaluator"></param>
        /// <param name="response"></param>
        /// <param name="history"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="SemanticAssertException"></exception>
        public async Task Assert(SemanticEvaluator semanticEvaluator, ChatResponse response, IReadOnlyList<ChatMessage>? history = null, CancellationToken cancellationToken = default)
        {
            if (Spec is null)
                throw new InvalidOperationException("FunctionCall assertion spec is not parsed.");

            var functionCalls = response.Messages
                                        .Where(
                                            ch => ch.Contents.OfType<FunctionCallContent>().Any()
                                        )
                                        .SelectMany(ch => ch.Contents.OfType<FunctionCallContent>())
                                        .ToList();

            var thisFunctionCalls = functionCalls
                                    .Where(fc => fc.Name == Spec.FunctionName)
                                    .ToList();

            if (thisFunctionCalls.Count == 0)
                throw new SemanticAssertException(
                    $"""
                     No function call found with name: {Spec.FunctionName}
                     Current calls: {string.Join(", ", functionCalls.Select(fc => fc.Name))}
                     """);

            if (Spec.MatchStrategy == FunctionCallMatchStrategy.ExactSingleCall && thisFunctionCalls.Count != 1)
            {
                throw new SemanticAssertException(
                    $"""
                     Expected exactly one call to {Spec.FunctionName}, but found {thisFunctionCalls.Count}.
                     """);
            }

            if (Spec.ArgumentAssertions.Any())
            {
                var thisFunctionCall = Spec.MatchStrategy == FunctionCallMatchStrategy.AnyCall
                    ? thisFunctionCalls.First()
                    : thisFunctionCalls.Last();

                var thisCallResult = (
                    from fr in response.Messages.SelectMany(c => c.Contents).OfType<FunctionResultContent>()
                    where fr.CallId == thisFunctionCall.CallId
                    select fr
                ).FirstOrDefault();

                if (thisCallResult is null)
                    throw new SemanticAssertException(
                        $"""
                         No function call result found with name: {Spec.FunctionName}
                         """);

                foreach (var argumentAssertion in Spec.ArgumentAssertions)
                {
                    var arguments = thisFunctionCall.Arguments ?? new Dictionary<string, object?>();

                    if (arguments.TryGetValue(argumentAssertion.Key, out var value))
                    {
                        var assertion = argumentAssertion.Value;
                        var actualValue = value?.ToString();

                        await assertion.Assert(
                            semanticEvaluator,
                            new ChatResponse(new ChatMessage(ChatRole.Assistant, actualValue)),
                            cancellationToken: cancellationToken);
                    }
                    else
                    {
                        throw new SemanticAssertException(
                            $"""
                             Argument {argumentAssertion.Key} is not found in the function call.
                             """);
                    }
                }
            }
        }

        public string AssertionType => "FunctionCall";

        public string Description => RawSpec;

        public override string ToString() => $"{AssertionType}: {RawSpec}";
    }
}
