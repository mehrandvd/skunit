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
        /// <param name="response"></param>
        /// <param name="history"></param>
        /// <returns></returns>
        /// <exception cref="SemanticAssertException"></exception>
        public Task Assert(Semantic semantic, ChatResponse response, IList<ChatMessage>? history = null)
        {
            if (!string.IsNullOrWhiteSpace(response.Text))
            {
                throw new SemanticAssertException($"""
                                    Expected to be empty, but not empty:
                                    {response.Text} 
                                    """);
            }

            return Task.CompletedTask;
        }

        public string AssertionType => "Empty";
        public string Description => "Empty";

        public override string ToString() => $"{AssertionType}";
    }
}
