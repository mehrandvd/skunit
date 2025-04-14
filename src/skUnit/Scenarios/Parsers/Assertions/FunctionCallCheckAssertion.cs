using SemanticValidation;
using skUnit.Exceptions;
using System.Text.Json.Nodes;
using SemanticValidation.Utils;
using Microsoft.Extensions.AI;
using FunctionCallContent = Microsoft.Extensions.AI.FunctionCallContent;
using FunctionResultContent = Microsoft.Extensions.AI.FunctionResultContent;
using Markdig.Helpers;

namespace skUnit.Scenarios.Parsers.Assertions
{
    public class FunctionCallAssertion : IKernelAssertion
    {
        // private readonly ArgumentConditionFactory factory = new();

        /// <summary>
        /// The expected conditions for a json answer.
        /// </summary>
        /// 
        private string FunctionCallText { get; set; } = default!;

        public JsonObject? FunctionCallJson { get; set; }

        public string? FunctionName { get; set; }

        public Dictionary<string, IKernelAssertion> FunctionArguments { get; set; } = new();

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
                        string checkValuesText;
                        string checkType;

                        if (kv.Value is JsonValue checkValue)
                        {
                            checkType = "Equals";
                            checkValuesText = checkValue.GetValue<string>();
                        }
                        else if (kv.Value is JsonArray checkArray)
                        {
                            //var checkArray = kv.Value.AsArray();
                            checkType = checkArray[0]?.GetValue<string>() ?? throw new InvalidOperationException("No valid array assertion.");

                            var checkValues = checkArray
                                              .Skip(1)
                                              .Select(value => value.GetValue<string>());

                            checkValuesText = string.Join(", ", checkValues);
                        }
                        else
                        {
                            throw new InvalidOperationException(
                                $"""
                                JsonCheck has not a proper value supported json types are string and array:
                                {kv.ToString()}
                                """);
                        }

                        FunctionArguments[kv.Key] = KernelAssertionParser.Parse(checkValuesText, checkType);
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
        /// <param name="response"></param>
        /// <param name="history"></param>
        /// <returns></returns>
        /// <exception cref="SemanticAssertException"></exception>
        public async Task Assert(Semantic semantic, ChatResponse response, IList<ChatMessage>? history = null)
        {
            if (FunctionName is null)
                throw new InvalidOperationException("FunctionCall Name is null");


            var functionCalls = response.Messages
                                        .Where(
                                            ch => ch.Contents.OfType<FunctionCallContent>().Any()
                                        )
                                        .SelectMany(ch => ch.Contents.OfType<FunctionCallContent>())
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
                var thisFunctionCall = thisFunctionCalls.Last();

                var thisCallResult = (
                    from fr in response.Messages.SelectMany(c => c.Contents).OfType<FunctionResultContent>()
                    where fr.CallId == thisFunctionCall.CallId
                    select fr
                ).FirstOrDefault();

                if (thisCallResult is null)
                    throw new SemanticAssertException(
                        $"""
                         No function call result found with name: {FunctionName}
                         """);

                foreach (var argumentAssertion in FunctionArguments)
                {
                    var arguments = thisFunctionCall.Arguments ?? new Dictionary<string, object?>();

                    if (arguments.TryGetValue(argumentAssertion.Key, out var value))
                    {
                        var assertion = argumentAssertion.Value;
                        var actualValue = value?.ToString();

                        await assertion.Assert(semantic, new ChatResponse(new ChatMessage(ChatRole.Assistant, actualValue)));
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

        public string Description => FunctionCallText;

        public override string ToString() => $"{AssertionType}: {FunctionCallText}";
    }
}
