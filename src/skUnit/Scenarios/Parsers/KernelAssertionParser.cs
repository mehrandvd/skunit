using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using skUnit.Scenarios.Parsers.Assertions;

namespace skUnit.Scenarios.Parsers
{
    public class KernelAssertionParser
    {
        public static IKernelAssertion Parse(string text, string type)
        {
            return type.ToLower() switch
            {
                "condition" => new HasConditionAssertion() { Condition = text },
                "similar" => new AreSameAssertion() { ExpectedAnswer = text },
                "contains" => new ContainsAllAssertion() { Texts = text.Split(',') },
                "containsall" => new ContainsAllAssertion() { Texts = text.Split(',') },
                "containsany" => new ContainsAnyAssertion() { Texts = text.Split(',') },
                _ => throw new InvalidOperationException($"Not valid assert type: {type}")
            };
        }
    }
}
