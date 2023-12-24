using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using skUnit.Parsers.Assertions;

namespace skUnit.Parsers
{
    public class KernelAssertParser
    {
        public static IKernelAssertion Parse(string text, string type)
        {
            return type.ToLower() switch
            {
                "condition" => new HasConditionAssertion() { Condition = text },
                "same" => new AreSameAssertion() { ExpectedAnswer = text },
                _ => throw new InvalidOperationException($"Not valid assert type: {type}")
            };
        }
    }
}
