using SemanticValidation;
using skUnit.Exceptions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using SemanticValidation.Utils;
using Microsoft.Extensions.AI;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel;
using FunctionResultContent = Microsoft.Extensions.AI.FunctionResultContent;

namespace skUnit.Scenarios.Parsers.Assertions
{
    public class FunctionCallAssertion : IKernelAssertion
    {
        /// <summary>
        /// The expected conditions for a json answer.
        /// </summary>
        private string FunctionCallText { get; set; } = default!;
        public JsonObject? FunctionCallJson { get; set; }
        public string? FunctionName { get; set; }

        public FunctionCallAssertion SetJsonAssertText(string jsonAssertText)
        {
            if (string.IsNullOrWhiteSpace(jsonAssertText))
                throw new InvalidOperationException("The FunctionCallCheck is empty.");

            FunctionCallText = (jsonAssertText ?? "");

            try
            {
                var json = SemanticUtils.PowerParseJson<JsonObject>(FunctionCallText);
                FunctionCallJson = json;
            }
            catch
            {
                // So it's a raw function name.
            }

            FunctionName = FunctionCallJson is not null 
                ? FunctionCallJson["function_name"]?.GetValue<string>() 
                : FunctionCallText;

            return this;
        }

        /// <summary>
        /// Checks if <paramref name="answer"/> is meets the conditions in FunctionCallJson <paramref name="semantic"/>
        /// </summary>
        /// <param name="semantic"></param>
        /// <param name="input"></param>
        /// <param name="historytory"></param>
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

        public string AssertionType => "FunctionCall";
        public string Description => FunctionCallText;

        public override string ToString() => $"{AssertionType}: {FunctionCallText}";
    }
}
