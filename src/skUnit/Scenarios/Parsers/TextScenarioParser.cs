using Markdig.Syntax;
using Markdig;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using skUnit.Scenarios;
using System.Diagnostics.CodeAnalysis;

namespace skUnit.Scenarios.Parsers
{
    public static class TextScenarioParser
    {
        public static List<TextScenario> Parse(string text, string config)
        {
            var scenarioTexts = Regex.Split(text, Environment.NewLine + @"-{5,}" + Environment.NewLine, RegexOptions.Multiline);
            var testCases = new List<TextScenario>();
            foreach (var scenarioText in scenarioTexts)
            {
                string? currentBlock = null;
                string? key = null;
                var scenario = new TextScenario() { RawText = scenarioText };
                testCases.Add(scenario);
                var specialId = "";

                var md = Markdown.Parse(scenarioText);
                var contentBuilder = new StringBuilder();

                if (!scenarioText.StartsWith("# TEST") && !scenarioText.StartsWith("## PARAMETER"))
                {
                    key = "input";
                    currentBlock = "PARAMETER";
                }

                foreach (var block in md)
                {
                    var blockContent = scenarioText.Substring(block.Span.Start, block.Span.Length);

                    if (block is HeadingBlock)
                    {
                        var testInfoMatch = Regex.Match(blockContent, @"#\s*(?<specialId>.*)?\s*TEST\s*(?<description>.*)");
                        if (testInfoMatch.Success)
                        {
                            specialId = testInfoMatch.Groups["specialId"].Value.Trim();
                            PackBlock(scenario, "TEST", ref currentBlock, key, contentBuilder);
                            contentBuilder.Append(testInfoMatch.Groups["description"].Value);
                            continue;
                        }

                        var promptMatch = Regex.Match(blockContent, @$"##\s*{specialId}\s*PROMPT\s*(?<name>.*)");
                        if (promptMatch.Success)
                        {
                            PackBlock(scenario, "PROMPT", ref currentBlock, key, contentBuilder);
                            //key = promptMatch.Groups["name"].Value;
                            continue;
                        }

                        var paramMatch = Regex.Match(blockContent, @$"##\s*{specialId}\s*PARAMETER\s*(?<param>.*)");
                        if (paramMatch.Success)
                        {
                            PackBlock(scenario, "PARAMETER", ref currentBlock, key, contentBuilder);
                            key = paramMatch.Groups["param"].Value;
                            continue;
                        }

                        var answerMatch = Regex.Match(blockContent, @$"##\s*{specialId}\s*ANSWER\s*(?<type>.*)");
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

                        var checkMatch = Regex.Match(blockContent, @$"###\s*{specialId}\s*CHECK\s*(?<type>.*)");
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
