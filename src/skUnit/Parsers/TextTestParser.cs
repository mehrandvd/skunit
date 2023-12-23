using Markdig.Syntax;
using Markdig;
using skUnit.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace skUnit.Parsers
{
    public static class TextTestParser
    {
        public static List<TextTestCase> Parse(string text, string config)
        {
            var testCaseTexts = Regex.Split(text, Environment.NewLine + @"-{5,}" + Environment.NewLine, RegexOptions.Multiline);
            var testCases = new List<TextTestCase>();
            foreach (var testText in testCaseTexts)
            {
                string? currentBlock = null;
                string? key = null;
                var testCase = new TextTestCase() { RawText = testText };
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

                        var paramMatch = Regex.Match(blockContent, @"##\s*PARAMETER:\s*(?<param>.*)");
                        if (paramMatch.Success)
                        {
                            PackBlock(testCase, "PARAMETER", ref currentBlock, key, contentBuilder);
                            key = paramMatch.Groups["param"].Value;
                            continue;
                        }

                        var assertMatch = Regex.Match(blockContent, @"##\s*ASSERT\s*(?<type>.*)");
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

            return testCases;
        }

        private static bool PackBlock(TextTestCase testCase, string newBlock, ref string? currentBlock, string? key, StringBuilder content)
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
                testCase.Arguments[key] = contentText;
            }
            else if (currentBlock == "TEST")
            {
                testCase.Description = contentText;
            }
            else if (currentBlock == "ASSERT")
            {
                testCase.Asserts.Add(KernelAssertParser.Parse(contentText, key ?? "semantic"));
            }

            currentBlock = newBlock;

            return true;
        }
    }
}
