using SemanticValidation;
using skUnit.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        /// <param name="answer"></param>
        /// <returns></returns>
        /// <exception cref="SemanticAssertException"></exception>
        public async Task Assert(Semantic semantic, string input)
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
