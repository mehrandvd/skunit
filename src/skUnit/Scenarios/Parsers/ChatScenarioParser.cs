using Markdig.Syntax;
using Markdig;
using System.Text;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using SemanticValidation.Utils;
using Microsoft.Extensions.AI;

namespace skUnit.Scenarios.Parsers
{
    public class ChatScenarioParser : IScenarioParser<ChatScenario>
    {
        /// <summary>
        /// Parses a ChatScenario from <paramref name="text"/>
        /// </summary>
        /// <param name="text"></param>
        /// <param name="config"></param>
        /// <returns></returns>
        public List<ChatScenario> Parse(string text, string config)
        {
            var scenarioTexts = Regex.Split(text, Environment.NewLine + @"-{5,}" + Environment.NewLine, RegexOptions.Multiline);
            var scenarios = new List<ChatScenario>();
            foreach (var testText in scenarioTexts)
            {
                string? currentBlock = null;
                string? key = null;
                var testCase = new ChatScenario() { RawText = testText };
                scenarios.Add(testCase);
                var specialId = "";

                var md = Markdown.Parse(testText);
                var contentBuilder = new StringBuilder();

                foreach (var block in md)
                {
                    var blockContent = testText.Substring(block.Span.Start, block.Span.Length);

                    if (block is HeadingBlock)
                    {
                        var testInfoMatch = Regex.Match(blockContent, @$"#{{1,}}\s*(?<specialId>.*)?\s*SCENARIO\s*(?<description>.*)");
                        if (testInfoMatch.Success)
                        {
                            specialId = testInfoMatch.Groups["specialId"].Value.Trim();
                            PackBlock(testCase, "SCENARIO", ref currentBlock, key, contentBuilder);
                            contentBuilder.Append(testInfoMatch.Groups["description"].Value);
                            continue;
                        }

                        var userMatch = Regex.Match(blockContent, @$"#{{1,}}\s*{specialId}\s*\[USER\]\s*(?<name>.*)");
                        if (userMatch.Success)
                        {
                            PackBlock(testCase, "USER", ref currentBlock, key, contentBuilder);
                            //key = promptMatch.Groups["name"].Value;
                            continue;
                        }

                        var agentMatch = Regex.Match(blockContent, @$"#{{1,}}\s*{specialId}\s*\[AGENT\]\s*(?<name>.*)");
                        if (agentMatch.Success)
                        {
                            PackBlock(testCase, "AGENT", ref currentBlock, key, contentBuilder);
                            //key = promptMatch.Groups["name"].Value;
                            continue;
                        }

                        var callMatch = Regex.Match(blockContent, @$"#{{1,}}\s*{specialId}\s*CALL\s*(?<functionCall>.*)");
                        if (callMatch.Success)
                        {
                            PackBlock(testCase, "CALL", ref currentBlock, key, contentBuilder);
                            key = callMatch.Groups["functionCall"].Value;
                            continue;
                        }

                        var answerMatch = Regex.Match(blockContent, @$"#{{1,}}\s*{specialId}\s*CHECK\s*(?<type>.*)");
                        if (answerMatch.Success)
                        {
                            PackBlock(testCase, "CHECK", ref currentBlock, key, contentBuilder);
                            key = answerMatch.Groups["type"].Value;
                            if (string.IsNullOrWhiteSpace(key))
                            {
                                key = null;
                            }

                            continue;
                        }
                    }

                    contentBuilder.AppendLine(blockContent);
                }

                PackBlock(testCase, "END", ref currentBlock, key, contentBuilder);

            }

            return scenarios;
        }

        private static bool PackBlock(ChatScenario scenario, string newBlock, ref string? currentBlock, string? key, StringBuilder content)
        {
            if (currentBlock is null)
            {
                currentBlock = newBlock;
                return false;
            }

            var contentText = content.ToString().Trim();
            content.Clear();

            if (currentBlock == "USER")
            {
                scenario.ChatItems.Add(new ChatItem(ChatRole.User, contentText));
            }
            else if (currentBlock == "AGENT")
            {
                scenario.ChatItems.Add(new ChatItem(ChatRole.Assistant, contentText));
            }
            else if (currentBlock == "SYSTEM")
            {
                scenario.ChatItems.Add(new ChatItem(ChatRole.System, contentText));
            }
            else if (currentBlock == "TOOL")
            {
                scenario.ChatItems.Add(new ChatItem(ChatRole.Tool, contentText));
            }
            else if (currentBlock == "SCENARIO")
            {
                scenario.Description = contentText;
            }
            else if (currentBlock == "CALL")
            {
                //var match = Regex.Match(key, @"(?<function>.*)\((?<args>.*)\)");

                //if (!match.Success)
                //    throw new InvalidOperationException($"Call is not valid: {key}");

                var functionText = key;

                if (string.IsNullOrWhiteSpace(functionText))
                {
                    throw new InvalidOperationException($"CALL function name is null");
                }

                var callParts = functionText.Split('.');
                if (callParts.Length != 2)
                {
                    throw new InvalidOperationException($"""
                        Invalid function call. It should be in Plugin.Function format:
                        { functionText} 
                        """ );
                }

                var plugin = callParts[0];
                var function = callParts[1];

                var arguments = new List<FunctionCallArgument>();

                if (!string.IsNullOrWhiteSpace(contentText))
                {
                    var argsJson = SemanticUtils.PowerParseJson<JsonObject>(contentText);

                    if (argsJson is null)
                        throw new InvalidOperationException($"""
                                Unable to parse CALL JSON:
                                { contentText} 
                                """ );

                    foreach (var prop in argsJson)
                    {
                        var argument = new FunctionCallArgument()
                        {
                            Name = prop.Key,
                        };
                        var propValue = prop.Value?.GetValue<string>() ?? "";

                        if (propValue.StartsWith("$"))
                        {
                            argument.InputVariable = propValue.Trim('$', ' ');
                        }
                        else
                        {
                            argument.LiteralValue = propValue;
                        }

                        arguments.Add(argument);
                    }
                }

                scenario.ChatItems.Last().FunctionCalls.Add(new FunctionCall()
                {
                    PluginName = plugin,
                    FunctionName = function,
                    Arguments = arguments,
                    ArgumentsText = contentText
                });
            }
            else if (currentBlock == "CHECK")
            {
                var chatItem = scenario.ChatItems.Last();
                var checkType = key ?? "SemanticSimilar";
                var checkText = contentText;

                if (string.IsNullOrWhiteSpace(checkText) && checkType == "SemanticSimilar")
                {
                    checkText = chatItem.Content;
                }

                var lastFunctionCall = chatItem.FunctionCalls.LastOrDefault();

                var assertion = KernelAssertionParser.Parse(checkText, checkType);

                if (lastFunctionCall != null)
                {
                    lastFunctionCall.Assertions.Add(assertion);
                }
                else
                {
                    chatItem.Assertions.Add(assertion);
                }
            }

            currentBlock = newBlock;

            return true;
        }
    }
}
