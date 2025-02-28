using SemanticValidation;
using skUnit.Exceptions;
using System.Text.Json.Nodes;
using SemanticValidation.Utils;
using Microsoft.Extensions.AI;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel;
using FunctionCallContent = Microsoft.Extensions.AI.FunctionCallContent;
using FunctionResultContent = Microsoft.Extensions.AI.FunctionResultContent;
using skUnit.Scenarios.Parsers.Assertions.FunctionArgumentConditions;

namespace skUnit.Scenarios.Parsers.Assertions
{
    public class FunctionCallAssertion : IKernelAssertion
    {
        private readonly ArgumentConditionFactory factory = new();

        /// <summary>
        /// The expected conditions for a json answer.
        /// </summary>
        /// 
        private string FunctionCallText { get; set; } = default!;

        public JsonObject? FunctionCallJson { get; set; }

        public string? FunctionName { get; set; }

        public Dictionary<string, IArgumentCondition> FunctionArguments { get; set; } = new();

        public FunctionCallAssertion SetJsonAssertText(string jsonAssertText)
        {
            if (string.IsNullOrWhiteSpace(jsonAssertText))
                throw new InvalidOperationException("The FunctionCallCheck is empty.");

            FunctionCallText = jsonAssertText ?? "";

            try
            {
                var json = SemanticUtils.PowerParseJson<JsonObject>(FunctionCallText);
                FunctionCallJson = json;
            }
            catch
            {
                // So it's a raw function name.
            }

            if (FunctionCallJson is not null)
            {
                FunctionName = FunctionCallJson["function_name"]?.GetValue<string>();

                if (FunctionCallJson["arguments"] is JsonObject argumentsJson)
                {
                    foreach (var kv in argumentsJson)
                    {
                        FunctionArguments[kv.Key] = factory.Create(kv.Value);
                    }
                }
            }
            else
            {
                FunctionName = FunctionCallText;
            }

            return this;
        }

        /// <summary>
        /// Checks if <paramref name="answer"/> is meets the conditions in FunctionCallJson <paramref name="semantic"/>
        /// </summary>
        /// <param name="semantic"></param>
        /// <param name="input"></param>
        /// <param name="history"></param>
        /// <returns></returns>
        /// <exception cref="SemanticAssertException"></exception>
        public async Task Assert(Semantic semantic, string input, IEnumerable<object>? history = null)
        {
            if (FunctionName is null)
                throw new InvalidOperationException("FunctionCall Name is null");


            if (history is IList<ChatMessage> chatMessageHistory)
            {
                var functionCalls = chatMessageHistory
                                    .Where(
                                        ch => ch.Role == ChatRole.Tool
                                              && ch.Contents.OfType<FunctionResultContent>().Any()
                                    )
                                    .SelectMany(ch => ch.Contents.OfType<FunctionResultContent>())
                                    .ToList();

                var thisFunctionCalls = functionCalls
                                        .Where(fc => fc.Name == FunctionName)
                                        .ToList();

                if (thisFunctionCalls.Count == 0)
                    throw new SemanticAssertException(
                        $"""
                         No function call found with name: {FunctionName}
                         Current calls: {string.Join(", ", functionCalls.Select(fc => fc.Name))}
                         """);


                if (FunctionArguments.Any())
                {
                    var lastFunctionCall = thisFunctionCalls.Last();

                    var relatedCall = (
                        from call in chatMessageHistory.SelectMany(c => c.Contents).OfType<FunctionCallContent>()
                        where call.CallId == lastFunctionCall.CallId
                        select call
                    ).FirstOrDefault();

                    if (relatedCall is null)
                        throw new SemanticAssertException(
                            $"""
                             No function call found with name: {FunctionName}
                             Current calls: {string.Join(", ", functionCalls.Select(fc => fc.Name))}
                             """);

                    foreach (var argument in FunctionArguments)
                    {
                        var realCallArguments = relatedCall.Arguments ?? new Dictionary<string, object?>();

                        if (realCallArguments.TryGetValue(argument.Key, out var value))
                        {
                            var condition = argument.Value;
                            if (condition is ISemanticArgumentCondition semanticCondition)
                                semanticCondition.Semantic = semantic;

                            string actualValue = value?.ToString();
                            if (!condition.IsMatch(actualValue))
                                throw CreateArgumentException(condition, argument.Key, actualValue);
                        }
                        else
                        {
                            throw new SemanticAssertException(
                                $"""
                                 Argument {argument.Key} is not found in the function call.
                                 """);
                        }
                    }
                }
            }
            else if(history is IList<ChatMessageContent> chatHistory)
            {
                var functionCalls = chatHistory
                                    .Where(
                                        ch => ch.Role == AuthorRole.Tool
                                        && ch.Items.OfType<Microsoft.SemanticKernel.FunctionResultContent>().Any()
                                    )
                                    .SelectMany(ch => ch.Items.OfType<Microsoft.SemanticKernel.FunctionResultContent>())
                                    .ToList();

                var thisFunctionCalls = functionCalls
                                        .Where(fc => fc.FunctionName == FunctionName)
                                        .ToList();

                if (thisFunctionCalls.Count == 0)
                    throw new SemanticAssertException(
                        $"""
                         No function call found with name: {FunctionName}
                         Current calls: {string.Join(", ", functionCalls.Select(fc => $"{fc.PluginName}-{fc.FunctionName}"))}
                         """);
            }
        }

        private SemanticAssertException CreateArgumentException(IArgumentCondition condition, string key, string value)
        {
            if (condition.Name == Conditions.NotEmpty)
                return new SemanticAssertException(
                    $"""
                    Argument `{key}` is expected to satisfy condition `{condition.Name}`, but it does not.
                    Actual value: `{value}`.
                    """);

            return new SemanticAssertException(
                $"""
                Argument `{key}` is expected to satisfy condition `{condition.Name}` with values: [{string.Join(", ", condition.ConditionValues.Select(value => $"`{value}`"))}], but it does not.
                Actual value: `{value}`.
                """);
        }

        public string AssertionType => "FunctionCall";

        public string Description => FunctionCallText;

        public override string ToString() => $"{AssertionType}: {FunctionCallText}";
    }
}
