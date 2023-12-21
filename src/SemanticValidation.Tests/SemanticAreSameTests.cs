using Xunit.Abstractions;

namespace SemanticValidation.Tests
{
    public class SemanticAreSameTests
    {
        private Semantic Semantic { get; set; }

        private ITestOutputHelper Output { get; set; }
        public SemanticAreSameTests(ITestOutputHelper output)
        {
            Output = output;

            var apiKey =
                Environment.GetEnvironmentVariable("openai-api-key", EnvironmentVariableTarget.User) ??
                throw new Exception("No ApiKey in environment variables.");
            var endpoint =
                Environment.GetEnvironmentVariable("openai-endpoint", EnvironmentVariableTarget.User) ??
                throw new Exception("No Endpoint in environment variables.");

            Semantic = new Semantic(endpoint, apiKey);
        }

        [Theory]
        [MemberData(nameof(GetSameData))]
        public async Task AreSame_True_MustWork(string first, string second)
        {
            var result = await Semantic.AreSameAsync(first, second);
            Assert.True(result.Success, result.Message);
        }

        [Theory]
        [MemberData(nameof(GetNotSameData))]
        public async Task AreSame_False_MustWork(string first, string second)
        {
            var result = await Semantic.AreSameAsync(first, second);
            Assert.False(result.Success);
            Output.WriteLine($"""
                [Explanation]
                {result.Message}
                """);
        }

        public static IEnumerable<object[]> GetNotSameData()
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

        public static IEnumerable<object[]> GetSameData()
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