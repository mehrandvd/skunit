using System.Text.Json;
using System.Text.Json.Nodes;
using Google.Protobuf.Reflection;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using skUnit.Exceptions;
using skUnit.Runners;

namespace skUnit
{
    /// <summary>
	/// Contains various methods that are used to semantically verify that conditions are met during the
	/// process of running tests. This class uses SemanticKernel and OpenAI to validate these assertions semantically.
	/// </summary>
    public class SemanticAssert
    {
        private SemanticAgent Semantic { get; set; }

        /// <summary>
        /// This class needs a SemanticKernel chatClient to work.
        /// Pass your pre-configured chatClient to this constructor.
        /// </summary>
        /// <param name="chatClient"></param>
        public SemanticAssert(IChatClient chatClient)
        {
            Semantic = new SemanticAgent(chatClient);
        }

        /// <summary>
        /// This class needs a Microsoft Agent Framework AI agent to work.
        /// Pass your pre-configured agent to this constructor.
        /// </summary>
        /// <param name="agent">The AI agent used for semantic evaluations and assertions.</param>
        public SemanticAssert(AIAgent agent)
        {
            Semantic = new SemanticAgent(agent);
        }

        /// <summary>
        /// Checks whether <paramref name="first"/> and <paramref name="second"/> string are semantically similar.
        /// It uses the kernel and OpenAI to check this semantically.
        /// <example>
        /// <code>
        /// SemanticAssert.SimilarAsync("This automobile is red", "The car is red") // returns true
        /// SemanticAssert.SimilarAsync("This tree is red", "The car is red") // returns false
        /// </code>
        /// </example>
        /// </summary>
        /// <param name="first"></param>
        /// <param name="second"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException">If the OpenAI was unable to generate a valid response.</exception>
        public async Task SimilarAsync(string first, string second)
        {
            var result = await Semantic.AreSimilarAsync(first, second);

            if (result is null)
            {
                throw new InvalidOperationException("Unable to accomplish the semantic assert.");
            }

            if (!result.IsValid)
            {
                throw new SemanticAssertException(result.Reason ?? "No reason is provided.");
            }
        }

        /// <summary>
        /// Checks whether <paramref name="first"/> and <paramref name="second"/> string are semantically similar.
        /// It uses the kernel and OpenAI to check this semantically.
        /// <example>
        /// <code>
        /// SemanticAssert.Similar("This automobile is red", "The car is red") // returns true
        /// SemanticAssert.Similar("This tree is red", "The car is red") // returns false
        /// </code>
        /// </example>
        /// </summary>
        /// <param name="first"></param>
        /// <param name="second"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException">If the OpenAI was unable to generate a valid response.</exception>
        public void Similar(string first, string second)
        {
            SimilarAsync(first, second).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Checks whether <paramref name="first"/> and <paramref name="second"/> string are NOT semantically similar.
        /// It uses the kernel and OpenAI to check this semantically. It also describes the reason that they are not similar.
        /// <example>
        /// <code>
        /// SemanticAssert.NotSimilarAsync("This bicycle is red", "The car is red")
        /// // returns:
        /// {
        ///   IsValid: false,
        ///   Reason: "The first text describes a red bicycle, while the second text describes a red car. They are not semantically equivalent."
        /// }
        /// </code>
        /// </example>
        /// </summary>
        /// <param name="first"></param>
        /// <param name="second"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException">If the OpenAI was unable to generate a valid response.</exception>
        public async Task NotSimilarAsync(string first, string second)
        {
            var result = await Semantic.AreSimilarAsync(first, second);

            if (result is null)
            {
                throw new Exception("Unable to accomplish the semantic assert.");
            }

            if (result.IsValid)
            {
                throw new SemanticAssertException($"""
                    These are semantically similar:
                    [FIRST]: {first}
                    [SECOND]: {second} 
                    """);
            }
        }

        /// <summary>
        /// Checks whether <paramref name="first"/> and <paramref name="second"/> string are NOT semantically similar.
        /// It uses the kernel and OpenAI to check this semantically. It also describes the reason that they are not similar.
        /// <example>
        /// <code>
        /// SemanticAssert.NotSimilar("This bicycle is red", "The car is red")
        /// // returns:
        /// {
        ///   IsValid: false,
        ///   Reason: "The first text describes a red bicycle, while the second text describes a red car. They are not semantically equivalent."
        /// }
        /// </code>
        /// </example>
        /// </summary>
        /// <param name="first"></param>
        /// <param name="second"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException">If the OpenAI was unable to generate a valid response.</exception>
        public void NotSimilar(string first, string second)
        {
            NotSimilarAsync(first, second).GetAwaiter().GetResult();
        }
    }
}
