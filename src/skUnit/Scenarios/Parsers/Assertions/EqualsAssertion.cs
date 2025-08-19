using Microsoft.Extensions.AI;
using SemanticValidation;
using skUnit.Exceptions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace skUnit.Scenarios.Parsers.Assertions
{
    /// <summary>
    /// Checks if the input is equal to ExpectedAnswer
    /// </summary>
    public class EqualsAssertion : IKernelAssertion
    {
        /// <summary>
        /// The expected input.
        /// </summary>
        public required string ExpectedAnswer { get; set; }

        /// <summary>
        /// Checks if the <paramref name="input"/> equals to ExpectedAnswer/>.
        /// </summary>
        /// <param name="semantic"></param>
        /// <param name="input"></param>
        /// <param name="historytory"></param>
        /// <returns></returns>
        /// <exception cref="SemanticAssertException"></exception>
        public Task Assert(Semantic semantic, ChatResponse response, IList<ChatMessage>? history)
        {
            if (response.Text.Trim() != ExpectedAnswer.Trim())
                throw new SemanticAssertException($"Expected input is: '{ExpectedAnswer}' while actual is : '{response.Text}'");

            return Task.CompletedTask;
        }

        public string AssertionType => "Equals";
        public string Description => ExpectedAnswer;

        public override string ToString() => $"{AssertionType}: {ExpectedAnswer}";
    }
}
