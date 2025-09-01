﻿using Markdig.Syntax;
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
        /// <returns></returns>
        public List<ChatScenario> Parse(string text)
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

                        var agentMatch = Regex.Match(blockContent, @$"#{{1,}}\s*{specialId}\s*\[(AGENT|ASSISTANT)\]\s*(?<name>.*)");
                        if (agentMatch.Success)
                        {
                            var blockType = agentMatch.Groups[1].Value; // Gets "AGENT" or "ASSISTANT"
                            PackBlock(testCase, blockType, ref currentBlock, key, contentBuilder);
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

                        var checkMatch = Regex.Match(blockContent, @$"#{{1,}}\s*{specialId}\s*CHECK\s*(?<type>.*)");
                        if (checkMatch.Success)
                        {
                            PackBlock(testCase, "CHECK", ref currentBlock, key, contentBuilder);
                            key = checkMatch.Groups["type"].Value;
                            if (string.IsNullOrWhiteSpace(key))
                            {
                                key = null;
                            }

                            continue;
                        }

                        var assertMatch = Regex.Match(blockContent, @$"#{{1,}}\s*{specialId}\s*ASSERT\s*(?<type>.*)");
                        if (assertMatch.Success)
                        {
                            PackBlock(testCase, "ASSERT", ref currentBlock, key, contentBuilder);
                            key = assertMatch.Groups["type"].Value;
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
                var contents = ParseMultiModalContent(contentText);
                scenario.ChatItems.Add(new ChatItem(ChatRole.User, contents));
            }
            else if (currentBlock == "AGENT" || currentBlock == "ASSISTANT")
            {
                var contents = ParseMultiModalContent(contentText);
                scenario.ChatItems.Add(new ChatItem(ChatRole.Assistant, contents));
            }
            else if (currentBlock == "SYSTEM")
            {
                var contents = ParseMultiModalContent(contentText);
                scenario.ChatItems.Add(new ChatItem(ChatRole.System, contents));
            }
            else if (currentBlock == "TOOL")
            {
                var contents = ParseMultiModalContent(contentText);
                scenario.ChatItems.Add(new ChatItem(ChatRole.Tool, contents));
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
                        {functionText} 
                        """);
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
                                {contentText} 
                                """);

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
            else if (currentBlock == "CHECK" || currentBlock == "ASSERT")
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

        /// <summary>
        /// Parse content that may contain multi-modal parts (### Text, ### Image sections)
        /// Falls back to treating the entire content as text if no multi-modal sections are found
        /// </summary>
        private static List<AIContent> ParseMultiModalContent(string contentText)
        {
            var contents = new List<AIContent>();

            // Check if content contains multi-modal subsections (### Text, ### Image)
            var hasMultiModalSections = Regex.IsMatch(contentText, @"^###\s*(Text|Image)\s*$", RegexOptions.Multiline | RegexOptions.IgnoreCase);

            if (!hasMultiModalSections)
            {
                // Backward compatibility: treat entire content as text
                if (!string.IsNullOrWhiteSpace(contentText))
                {
                    contents.Add(new TextContent(contentText));
                }
                return contents;
            }

            // Parse multi-modal sections
            var md = Markdown.Parse(contentText);
            var currentContentType = "";
            var sectionContent = new StringBuilder();

            foreach (var block in md)
            {
                var blockContent = contentText.Substring(block.Span.Start, block.Span.Length);

                if (block is HeadingBlock heading && heading.Level == 3)
                {
                    // Flush previous section if any
                    FlushContentSection(contents, currentContentType, sectionContent);

                    // Check for Text or Image subsection
                    var textMatch = Regex.Match(blockContent, @"^###\s*Text\s*$", RegexOptions.IgnoreCase);
                    var imageMatch = Regex.Match(blockContent, @"^###\s*Image\s*$", RegexOptions.IgnoreCase);

                    if (textMatch.Success)
                    {
                        currentContentType = "TEXT";
                    }
                    else if (imageMatch.Success)
                    {
                        currentContentType = "IMAGE";
                    }
                    else
                    {
                        // Not a recognized content type subsection, treat as regular content
                        currentContentType = "";
                        sectionContent.AppendLine(blockContent);
                    }
                }
                else
                {
                    sectionContent.AppendLine(blockContent);
                }
            }

            // Flush final section
            FlushContentSection(contents, currentContentType, sectionContent);

            return contents.Count > 0 ? contents : new List<AIContent> { new TextContent(contentText) };
        }

        private static void FlushContentSection(List<AIContent> contents, string contentType, StringBuilder sectionContent)
        {
            var content = sectionContent.ToString().Trim();
            sectionContent.Clear();

            if (string.IsNullOrWhiteSpace(content)) return;

            switch (contentType.ToUpperInvariant())
            {
                case "TEXT":
                    contents.Add(new TextContent(content));
                    break;
                case "IMAGE":
                    // Extract image URL from markdown image syntax: ![alt](url)
                    var imageMatch = Regex.Match(content, @"!\[([^\]]*)\]\(([^)]+)\)");
                    if (imageMatch.Success)
                    {
                        var imageUrl = imageMatch.Groups[2].Value;
                        contents.Add(new UriContent(new Uri(imageUrl), "image/*"));
                    }
                    else
                    {
                        // If not a proper markdown image, treat as text content
                        contents.Add(new TextContent(content));
                    }
                    break;
                default:
                    // Default to text content for any unrecognized or empty content type
                    contents.Add(new TextContent(content));
                    break;
            }
        }
    }
}
