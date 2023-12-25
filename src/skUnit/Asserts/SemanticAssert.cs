using System.Text.Json;
using System.Text.Json.Nodes;
using Json.More;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using SemanticValidation;
using skUnit.Exceptions;

namespace skUnit
{
    /// <summary>
	/// Contains various static methods that are used to semantically verify that conditions are met during the
	/// process of running tests. This class uses SemanticKernel and OpenAI to validate these assertions semantically.
	/// </summary>
    public class SemanticAssert
    {
        private static Semantic Semantic { get; set; } = default!;

        /// <summary>
        /// This class needs a SemanticKernel kernel to work.
        /// Using this constructor you can use an AzureOpenAI subscription to configure it.
        /// If you want to connect using an OpenAI client, you can configure your kernel
        /// as you like and pass your pre-configured kernel using the other constructor.
        /// </summary>
        /// <param name="deploymentName"></param>
        /// <param name="endpoint"></param>
        /// <param name="apiKey"></param>
        public static void Initialize(string deploymentName, string endpoint, string apiKey)
        {
            Semantic = new Semantic(deploymentName,endpoint, apiKey);

        }

        /// <summary>
        /// This class needs a SemanticKernel kernel to work.
        /// Pass your pre-configured kernel to this constructor.
        /// </summary>
        /// <param name="kernel"></param>
        public static void Initialize(Kernel kernel)
        {
            Semantic = new Semantic(kernel);

        }

        public static async Task SimilarAsync(string first, string second)
        {
            var result = await Semantic.AreSimilarAsync(first, second);

            if (result is null)
            {
                throw new SemanticAssertException("Unable to accomplish the semantic assert.");
            }

            if (!result.IsValid)
            {
                throw new SemanticAssertException(result.Reason ?? "No reason is provided.");
            }
        }

        public static void Similar(string first, string second)
        {
            SimilarAsync(first, second).GetAwaiter().GetResult();
        }

        public static async Task NotSimilarAsync(string first, string second)
        {
            var result = await Semantic.AreSimilarAsync(first, second);

            if (result is null)
            {
                throw new SemanticAssertException("Unable to accomplish the semantic assert.");
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

        public static void NotSimilar(string first, string second)
        {
            NotSimilarAsync(first, second).GetAwaiter().GetResult();
        }


        public static void Satisfies(string input, string condition)
        {

        }
    }
}
