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

namespace skUnit.Models
{
    public class TestCase
    {
        public required string RawText { get; set; }
        public Dictionary<string, string> Arguments { get; set; } = new();
        public List<IKernelAssert> Asserts { get; set; } = new();
    }

    public class TextTestCase
    {
        public string? Description { get; set; }
        public required string RawText { get; set; }
        public Dictionary<string, string> Arguments { get; set; } = new();
        public List<IKernelAssert> Asserts { get; set; } = new();
    }
}
