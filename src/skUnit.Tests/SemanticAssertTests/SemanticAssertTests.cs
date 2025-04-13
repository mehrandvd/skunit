using Azure.AI.OpenAI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using skUnit.Exceptions;
using Xunit.Abstractions;

namespace skUnit.Tests.SemanticAssertTests
{
    public class SemanticAssertTests
    {
        private ITestOutputHelper Output { get; set; }
        private SemanticAssert SemanticAssert { get; set; }

        public SemanticAssertTests(ITestOutputHelper output)
        {
            Output = output;

            var builder = new ConfigurationBuilder()
                .AddUserSecrets<SemanticAssertTests>();

            IConfiguration configuration = builder.Build();

            var apiKey =
                configuration["AzureOpenAI_ApiKey"] ??
                throw new Exception("No ApiKey is provided.");
            var endpoint =
                configuration["AzureOpenAI_Endpoint"] ??
                throw new Exception("No Endpoint is provided.");
            var deploymentName =
                configuration["AzureOpenAI_Deployment"] ??
                throw new Exception("No Deployment is provided.");

            SemanticAssert = new SemanticAssert(
                new AzureOpenAIClient(
                    new Uri(endpoint), 
                    new System.ClientModel.ApiKeyCredential(apiKey)
                    ).GetChatClient(deploymentName).AsIChatClient()
                );
        }

        [Theory]
        [MemberData(nameof(GetSimilarData))]
        public void Similar_True_MustWork(string first, string second)
        {
            SemanticAssert.Similar(first, second);
        }

        [Theory]
        [MemberData(nameof(GetNonSimilarData))]
        public void Similar_False_MustWork(string first, string second)
        {
            var exception = Assert.Throws<SemanticAssertException>(() => SemanticAssert.Similar(first, second));
            Output.WriteLine($"""
                [Explanation]
                {exception.Message}
                """);
        }

        [Theory]
        [MemberData(nameof(GetNonSimilarData))]
        public void NotSimilar_True_MustWork(string first, string second)
        {
            SemanticAssert.NotSimilar(first, second);
        }

        [Theory]
        [MemberData(nameof(GetSimilarData))]
        public void NotSimilar_False_MustWork(string first, string second)
        {
            var exception = Assert.Throws<SemanticAssertException>(() => SemanticAssert.NotSimilar(first, second));
            Output.WriteLine($"""
                [Explanation]
                {exception.Message}
                """);
        }

        public static IEnumerable<object[]> GetNonSimilarData()
        {
            yield return new object[]
            {
                "This car is red",
                "The car is blue"
            };
            yield return new object[]
            {
                "This bicycle is red",
                "The car is red"
            };
        }

        public static IEnumerable<object[]> GetSimilarData()
        {
            yield return new object[]
            {
                "This car is red",
                "The car is red"
            };
            yield return new object[]
            {
                "This automobile is red",
                "The car is red"
            };
        }
    }
}