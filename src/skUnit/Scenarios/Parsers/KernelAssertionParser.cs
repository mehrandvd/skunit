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
        /// <summary>
        /// Parses an assertion text to a related KernelAssertion. For example:
        /// <code>
        /// HasConditionAssertion, AreSimilarAssertion, ContainsAllAssertion
        /// </code>
        /// </summary>
        /// <param name="text"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public static IKernelAssertion Parse(string text, string type)
        {
            return type.Trim().ToLower() switch
            {
                "condition" 
                    => new HasConditionAssertion() { Condition = text },
                "similar" 
                    => new AreSimilarAssertion() { ExpectedAnswer = text },
                "contains" or "contain" or "containsall"
                    => new ContainsAllAssertion() { Texts = text.Split(',', '،') },
                "containsany" 
                    => new ContainsAnyAssertion() { Texts = text.Split(',', '،') },
                "equal" or "equals"
                    => new EqualsAssertion() { ExpectedAnswer = text },
                _ => throw new InvalidOperationException($"Not valid assert type: {type}")
            };
        }
    }
}
