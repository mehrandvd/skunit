using skUnit.Exceptions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.AI;
using skUnit.Runners;

namespace skUnit.Scenarios.Parsers.Assertions
{
    /// <summary>
    /// Checks if the input is empty
    /// </summary>
    public class EmptyAssertion : IChatAssertion
    {
        /// <summary>
        /// Checks if the <paramref name="answer"/> is empty/>.
        /// </summary>
        /// <param name="semantic"></param>
        /// <param name="response"></param>
        /// <param name="history"></param>
        /// <returns></returns>
        /// <exception cref="SemanticAssertException"></exception>
        public Task Assert(SemanticEvaluator semanticEvaluator, ChatResponse response, IReadOnlyList<ChatMessage>? history = null, CancellationToken cancellationToken = default)
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
