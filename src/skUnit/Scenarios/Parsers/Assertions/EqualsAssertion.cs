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
    /// Checks if the answer is equal to ExpectedAnswer
    /// </summary>
    public class EqualsAssertion : IKernelAssertion
    {
        /// <summary>
        /// The expected answer.
        /// </summary>
        public required string ExpectedAnswer { get; set; }

        /// <summary>
        /// Checks if the <paramref name="answer"/> equals to ExpectedAnswer/>.
        /// </summary>
        /// <param name="semantic"></param>
        /// <param name="answer"></param>
        /// <returns></returns>
        /// <exception cref="SemanticAssertException"></exception>
        public async Task Assert(Semantic semantic, string answer)
        {
            if (answer.Trim() != ExpectedAnswer.Trim())
                throw new SemanticAssertException($"Expected answer is: '{ExpectedAnswer}' while actual is : '{answer}'");
        }

        public string AssertionType => "Equals";
        public string Description => ExpectedAnswer;

        public override string ToString() => $"{AssertionType}: {ExpectedAnswer}";
    }
}
