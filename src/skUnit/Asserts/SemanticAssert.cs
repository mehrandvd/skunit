using System.Text.Json;
using System.Text.Json.Nodes;
using Json.More;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using SemanticValidation;
using skUnit.Exceptions;

namespace skUnit
{
    public class SemanticAssert
    {
        private static Semantic Semantic { get; set; } = default!;
        public static void Initialize(string deploymentName, string endpoint, string apiKey)
        {
            Semantic = new Semantic(deploymentName,endpoint, apiKey);

        }

        public static async Task SameAsync(string first, string second)
        {
            var result = await Semantic.AreSameAsync(first, second);

            if (result is null)
            {
                throw new SemanticAssertException("Unable to accomplish the semantic assert.");
            }

            if (!result.Success)
            {
                throw new SemanticAssertException(result.Message ?? "No reason is provided.");
            }
        }

        public static void Same(string first, string second)
        {
            SameAsync(first, second).GetAwaiter().GetResult();
        }

        public static async Task NotSameAsync(string first, string second)
        {
            var result = await Semantic.AreSameAsync(first, second);

            if (result is null)
            {
                throw new SemanticAssertException("Unable to accomplish the semantic assert.");
            }

            if (result.Success)
            {
                throw new SemanticAssertException($"""
                    These are semantically same:
                    [FIRST]: {first}
                    [SECOND]: {second} 
                    """);
            }
        }

        public static void NotSame(string first, string second)
        {
            NotSameAsync(first, second).GetAwaiter().GetResult();
        }


        public static void Satisfies(string input, string condition)
        {

        }
    }
}
