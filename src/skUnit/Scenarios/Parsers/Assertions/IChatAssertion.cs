using Microsoft.Extensions.AI;
using skUnit.Runners;

namespace skUnit.Scenarios.Parsers.Assertions
{
    /// <summary>
    /// An assertion that can be applied to the input returned by a kernel.
    /// </summary>
    public interface IChatAssertion
    {
        /// <summary>
        /// Checks if the <paramref name="response"/> can pass the assertion using <paramref name="semantic"/>
        /// </summary>
        /// <param name="semanticEvaluator"></param>
        /// <param name="response"></param>
        /// <param name="history"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task Assert(
            SemanticEvaluator semanticEvaluator,
            ChatResponse response,
            IReadOnlyList<ChatMessage>? history = null,
            CancellationToken cancellationToken = default);
        string AssertionType { get; }
        string Description { get; }
    }
}
