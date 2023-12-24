using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Markdig;
using Markdig.Parsers;
using Markdig.Syntax;
using Microsoft.VisualBasic.CompilerServices;
using skUnit.Parsers.Assertions;

namespace skUnit.Models
{
    public class Scenario
    {
        public required string RawText { get; set; }
        public Dictionary<string, string> Arguments { get; set; } = new();
        public List<IKernelAssertion> Asserts { get; set; } = new();
    }

    public class TextScenario
    {
        public string? Description { get; set; }
        public required string RawText { get; set; }
        public string? ExpectedAnswer { get; set; }
        public Dictionary<string, string> Arguments { get; set; } = new();
        public List<IKernelAssertion> Asserts { get; set; } = new();

    }
}
