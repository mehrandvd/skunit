using Markdig.Syntax;
using Markdig;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.SemanticKernel.ChatCompletion;
using skUnit.Scenarios;

namespace skUnit.Scenarios.Parsers
{
    public static class ChatScenarioParser
    {
        /// <summary>
        /// Parses a ChatScenario from <paramref name="text"/>
        /// </summary>
        /// <param name="text"></param>
        /// <param name="config"></param>
        /// <returns></returns>
        public static List<ChatScenario> Parse(string text, string config)
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
                        var testInfoMatch = Regex.Match(blockContent, @$"#{{1,}}\s*(?<specialId>.*)?\s*TEST\s*(?<description>.*)");
                        if (testInfoMatch.Success)
                        {
                            specialId = testInfoMatch.Groups["specialId"].Value.Trim();
                            PackBlock(testCase, "TEST", ref currentBlock, key, contentBuilder);
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
                scenario.ChatItems.Add(new ChatItem(AuthorRole.User, contentText));
            }
            else if (currentBlock == "AGENT")
            {
                scenario.ChatItems.Add(new ChatItem(AuthorRole.Assistant, contentText));
            }
            else if (currentBlock == "SYSTEM")
            {
                scenario.ChatItems.Add(new ChatItem(AuthorRole.System, contentText));
            }
            else if (currentBlock == "TOOL")
            {
                scenario.ChatItems.Add(new ChatItem(AuthorRole.Tool, contentText));
            }
            else if (currentBlock == "TEST")
            {
                scenario.Description = contentText;
            }
            else if (currentBlock == "CHECK")
            {
                var chatItem = scenario.ChatItems.Last();
                var checkType = key ?? "similar";
                var checkText = contentText;

                if (string.IsNullOrWhiteSpace(checkText) && checkType == "similar")
                {
                    checkText = chatItem.Content;
                }

                chatItem.Assertions.Add(KernelAssertionParser.Parse(checkText, checkType));
            }

            currentBlock = newBlock;

            return true;
        }
    }
}
