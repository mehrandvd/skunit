﻿using Markdig.Syntax;
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
    public class InvocationScenarioParser : IScenarioParser<InvocationScenario>
    {
        /// <summary>
        /// Parses an InvocationScenario from <paramref name="text"/>
        /// </summary>
        /// <param name="text"></param>
        /// <param name="config"></param>
        /// <returns></returns>
        public List<InvocationScenario> Parse(string text, string config)
        {
            var scenarioTexts = Regex.Split(text, Environment.NewLine + @"-{5,}" + Environment.NewLine, RegexOptions.Multiline);
            var scenarios = new List<InvocationScenario>();
            foreach (var scenarioText in scenarioTexts)
            {
                string? currentBlock = null;
                string? key = null;
                var scenario = new InvocationScenario() { RawText = scenarioText };
                scenarios.Add(scenario);
                var specialId = "";

                var md = Markdown.Parse(scenarioText);
                var contentBuilder = new StringBuilder();

                if (!scenarioText.StartsWith("# SCENARIO") && !scenarioText.StartsWith("## PARAMETER"))
                {
                    key = "input";
                    currentBlock = "PARAMETER";
                }

                foreach (var block in md)
                {
                    var blockContent = scenarioText.Substring(block.Span.Start, block.Span.Length);

                    if (block is HeadingBlock)
                    {
                        var testInfoMatch = Regex.Match(blockContent, @"#{1,}\s*(?<specialId>.*)?\s*SCENARIO\s*(?<description>.*)");
                        if (testInfoMatch.Success)
                        {
                            specialId = testInfoMatch.Groups["specialId"].Value.Trim();
                            PackBlock(scenario, "SCENARIO", ref currentBlock, key, contentBuilder);
                            contentBuilder.Append(testInfoMatch.Groups["description"].Value);
                            continue;
                        }

                        var promptMatch = Regex.Match(blockContent, @$"#{{1,}}\s*{specialId}\s*PROMPT\s*(?<name>.*)");
                        if (promptMatch.Success)
                        {
                            PackBlock(scenario, "PROMPT", ref currentBlock, key, contentBuilder);
                            //key = promptMatch.Groups["name"].Value;
                            continue;
                        }

                        var paramMatch = Regex.Match(blockContent, @$"#{{1,}}\s*{specialId}\s*PARAMETER\s*(?<param>.*)");
                        if (paramMatch.Success)
                        {
                            PackBlock(scenario, "PARAMETER", ref currentBlock, key, contentBuilder);
                            key = paramMatch.Groups["param"].Value;
                            continue;
                        }

                        var answerMatch = Regex.Match(blockContent, @$"#{{1,}}\s*{specialId}\s*ANSWER\s*(?<type>.*)");
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

                        var checkMatch = Regex.Match(blockContent, @$"#{{1,}}\s*{specialId}\s*CHECK\s*(?<type>.*)");
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

            return scenarios;
        }

        private static bool PackBlock(InvocationScenario scenario, string newBlock, ref string? currentBlock, string? key, StringBuilder content)
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
            else if (currentBlock == "SCENARIO")
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
                var checkType = key ?? "SemanticSimilar";
                var checkText = contentText;

                if (string.IsNullOrWhiteSpace(checkText) && checkType == "SemanticSimilar")
                {
                    checkText = scenario.ExpectedAnswer ?? "";
                }

                scenario.Assertions.Add(KernelAssertionParser.Parse(checkText, key ?? "SemanticSimilar"));
            }

            currentBlock = newBlock;

            return true;
        }
    }
}
