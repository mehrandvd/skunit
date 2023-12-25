using Markdig.Syntax;
using Markdig;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using skUnit.Scenarios;

namespace skUnit.Scenarios.Parsers
{
    public static class TextScenarioParser
    {
        public static List<TextScenario> Parse(string text, string config)
        {
            var testCaseTexts = Regex.Split(text, Environment.NewLine + @"-{5,}" + Environment.NewLine, RegexOptions.Multiline);
            var testCases = new List<TextScenario>();
            foreach (var testText in testCaseTexts)
            {
                string? currentBlock = null;
                string? key = null;
                var scenario = new TextScenario() { RawText = testText };
                testCases.Add(scenario);

                var md = Markdown.Parse(testText);
                var contentBuilder = new StringBuilder();

                if (!testText.StartsWith("# TEST") && !testText.StartsWith("## PARAMETER"))
                {
                    key = "input";
                    currentBlock = "PARAMETER";
                }

                foreach (var block in md)
                {
                    var blockContent = testText.Substring(block.Span.Start, block.Span.Length);

                    if (block is HeadingBlock)
                    {
                        var testInfoMatch = Regex.Match(blockContent, @"#\s*TEST\s*(?<description>.*)");
                        if (testInfoMatch.Success)
                        {
                            PackBlock(scenario, "TEST", ref currentBlock, key, contentBuilder);
                            contentBuilder.Append(testInfoMatch.Groups["description"].Value);
                            continue;
                        }

                        var promptMatch = Regex.Match(blockContent, @"##\s*PROMPT\s*(?<name>.*)");
                        if (promptMatch.Success)
                        {
                            PackBlock(scenario, "PROMPT", ref currentBlock, key, contentBuilder);
                            //key = promptMatch.Groups["name"].Value;
                            continue;
                        }

                        var paramMatch = Regex.Match(blockContent, @"##\s*PARAMETER\s*(?<param>.*)");
                        if (paramMatch.Success)
                        {
                            PackBlock(scenario, "PARAMETER", ref currentBlock, key, contentBuilder);
                            key = paramMatch.Groups["param"].Value;
                            continue;
                        }

                        var answerMatch = Regex.Match(blockContent, @"##\s*ANSWER\s*(?<type>.*)");
                        if (answerMatch.Success)
                        {
                            PackBlock(scenario, "ANSWER", ref currentBlock, key, contentBuilder);
                            key = answerMatch.Groups["type"].Value;
                            if (string.IsNullOrWhiteSpace(key))
                            {
                                key = null;
                            }

                            continue;
                        }

                        var checkMatch = Regex.Match(blockContent, @"###\s*CHECK\s*(?<type>.*)");
                        if (checkMatch.Success)
                        {
                            PackBlock(scenario, "CHECK", ref currentBlock, key, contentBuilder);
                            key = checkMatch.Groups["type"].Value;
                            if (string.IsNullOrWhiteSpace(key))
                            {
                                key = null;
                            }

                            continue;
                        }
                    }

                    contentBuilder.AppendLine(blockContent);
                }

                PackBlock(scenario, "END", ref currentBlock, key, contentBuilder);

            }

            return testCases;
        }

        private static bool PackBlock(TextScenario scenario, string newBlock, ref string? currentBlock, string? key, StringBuilder content)
        {
            if (currentBlock is null)
            {
                currentBlock = newBlock;
                return false;
            }

            var contentText = content.ToString().Trim();
            content.Clear();

            if (currentBlock == "PARAMETER")
            {
                scenario.Parameters[key] = contentText;
            }
            else if (currentBlock == "PROMPT")
            {
                scenario.Prompt = contentText;
            }
            else if (currentBlock == "TEST")
            {
                scenario.Description = contentText;
            }
            else if (currentBlock == "ANSWER")
            {
                if (!string.IsNullOrWhiteSpace(key))
                {
                    scenario.Assertions.Add(KernelAssertionParser.Parse(contentText, key));
                }

                scenario.ExpectedAnswer = contentText;
            }
            else if (currentBlock == "CHECK")
            {
                var checkType = key ?? "similar";
                var checkText = contentText;

                if (string.IsNullOrWhiteSpace(checkText) && checkType == "similar")
                {
                    checkText = scenario.ExpectedAnswer ?? "";
                }

                scenario.Assertions.Add(KernelAssertionParser.Parse(checkText, key ?? "similar"));
            }

            currentBlock = newBlock;

            return true;
        }
    }
}
