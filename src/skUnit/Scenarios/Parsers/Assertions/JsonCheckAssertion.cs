using SemanticValidation;
using skUnit.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using SemanticValidation.Utils;

namespace skUnit.Scenarios.Parsers.Assertions
{
    public class JsonCheckAssertion : IKernelAssertion
    {
        /// <summary>
        /// The expected conditions for a json answer.
        /// </summary>
        private string JsonCheckText { get; set; } = default!;
        public JsonObject? JsonCheck { get; set; }
        public JsonCheckAssertion SetJsonAssertText(string jsonAssertText)
        {
            if (string.IsNullOrWhiteSpace(jsonAssertText))
                throw new InvalidOperationException("The JsonCheck is empty.");

            JsonCheckText = (jsonAssertText ?? "");

            var json = SemanticUtils.PowerParseJson<JsonObject>(JsonCheckText);

            JsonCheck = json ?? throw new InvalidOperationException($"""
                    Can not parse json: 
                    {JsonCheckText}
                    """);

            return this;
        }

        /// <summary>
        /// Checks if <paramref name="answer"/> is meets the conditions in JsonCheck <paramref name="semantic"/>
        /// </summary>
        /// <param name="semantic"></param>
        /// <param name="answer"></param>
        /// <returns></returns>
        /// <exception cref="SemanticAssertException"></exception>
        public async Task Assert(Semantic semantic, string input)
        {
            var answerJson = SemanticUtils.PowerParseJson<JsonObject>(input)
                             ?? throw new InvalidOperationException($"""
                    Can not parse answer to json: 
                    {input}
                    """);

            if (JsonCheck is null)
                throw new InvalidOperationException("JsonCheck is null");

            foreach (var prop in JsonCheck)
            {
                var checkArray = prop.Value?.AsArray();

                if (checkArray is null || checkArray.Count > 2)
                    throw new InvalidOperationException($"""
                        JsonCheck has not a proper array, (it should have maximum 2 members): 
                        {checkArray?.ToJsonString()}
                        """);

                var check = checkArray[0]?.GetValue<string>();
                var body = checkArray.ElementAtOrDefault(1)?.GetValue<string>() ?? "";

                if (string.IsNullOrWhiteSpace(check))
                    throw new InvalidOperationException($"JsonCheck check field is empty: {checkArray?.ToJsonString()}");

                var assertion = KernelAssertionParser.Parse(body, check);

                if (answerJson.ContainsKey(prop.Key))
                {
                    var answerValue = answerJson[prop.Key]?.GetValue<string>() ?? "";
                    try
                    {
                        await assertion.Assert(semantic, answerValue);
                    }
                    catch (SemanticAssertException semanticAssertException)
                    {
                        throw new SemanticAssertException($"""
                            JsonCheck Assertion failed:
                            { checkArray?.ToJsonString()} 
                            DESCRIPTION:
                            { semanticAssertException.Message} 
                            """ );
                    }
                    catch (InvalidOperationException invalidOperationException)
                    {
                        throw new InvalidOperationException($"""
                            Invalid JsonCheck:
                            {checkArray?.ToJsonString()} 
                            DESCRIPTION:
                            {invalidOperationException.Message} 
                            """);
                    }
                }
                else
                {
                    throw new SemanticAssertException($"""
                            Property '{ prop.Key}  ' not found in answer:
                            {input} 
                            """ );
                }
            }
        }

        public string AssertionType => "JsonCheck";
        public string Description => JsonCheckText;

        public override string ToString() => $"{AssertionType}: {JsonCheckText}";
    }
}
