using Markdig.Syntax;
using Markdig;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using skUnit.Scenarios;

namespace skUnit.Parsers
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
                var testCase = new TextScenario() { RawText = testText };
                testCases.Add(testCase);

                var md = Markdown.Parse(testText);
                var contentBuilder = new StringBuilder();

                if (!testText.StartsWith("# TEST") && !testText.StartsWith("## PARAMETER:"))
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
                            PackBlock(testCase, "TEST", ref currentBlock, key, contentBuilder);
                            contentBuilder.Append(testInfoMatch.Groups["description"].Value);
                            continue;
                        }

                        var paramMatch = Regex.Match(blockContent, @"##\s*PARAMETER\s*(?<param>.*)");
                        if (paramMatch.Success)
                        {
                            PackBlock(testCase, "PARAMETER", ref currentBlock, key, contentBuilder);
                            key = paramMatch.Groups["param"].Value;
                            continue;
                        }

                        var answerMatch = Regex.Match(blockContent, @"##\s*ANSWER\s*(?<type>.*)");
                        if (answerMatch.Success)
                        {
                            PackBlock(testCase, "ANSWER", ref currentBlock, key, contentBuilder);
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

            return testCases;
        }

        private static bool PackBlock(TextScenario scenario, string newBlock, ref string? currentBlock, string? key, StringBuilder content)
        {
            if (currentBlock is null)
            {
                currentBlock = newBlock;
                return false;
            }

            var contentText = content.ToString();
            content.Clear();

            if (currentBlock == "PARAMETER")
            {
                scenario.Parameters[key] = contentText;
            }
            else if (currentBlock == "TEST")
            {
                scenario.Description = contentText;
            }
            else if (currentBlock == "ANSWER")
            {
                scenario.Assertions.Add(KernelAssertParser.Parse(contentText, key ?? "same"));
            }

            currentBlock = newBlock;

            return true;
        }
    }
}
