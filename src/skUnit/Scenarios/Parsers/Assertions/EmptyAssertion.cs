using SemanticValidation;
using skUnit.Exceptions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.AI;

namespace skUnit.Scenarios.Parsers.Assertions
{
    /// <summary>
    /// Checks if the input is empty
    /// </summary>
    public class EmptyAssertion : IKernelAssertion
    {
        /// <summary>
        /// Checks if the <paramref name="answer"/> is empty/>.
        /// </summary>
        /// <param name="semantic"></param>
        /// <param name="input"></param>
        /// <param name="historytory"></param>
        /// <param name="answer"></param>
        /// <returns></returns>
        /// <exception cref="SemanticAssertException"></exception>
        public async Task Assert(Semantic semantic, string input, IEnumerable<object>? history = null)
        {
            if (!string.IsNullOrWhiteSpace(input))
            {
                throw new SemanticAssertException($"""
                                    Expected to be empty, but not empty:
                                    {input} 
                                    """ );
            }
        }

        public string AssertionType => "Empty";
        public string Description => "Empty";

        public override string ToString() => $"{AssertionType}";
    }
}
