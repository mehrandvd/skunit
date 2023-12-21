using Xunit.Abstractions;

namespace SemanticValidation.Tests
{
    public class SemanticHasConditionTests
    {
        private ITestOutputHelper Output { get; set; }
        private Semantic Semantic { get; set; }
        public SemanticHasConditionTests(ITestOutputHelper output)
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
        [MemberData(nameof(GetHasConditionData))]
        public async Task HasCondition_True_MustWork(string text, string condition)
        {
            var result = await Semantic.HasConditionAsync(text, condition);
            Assert.True(result.Success, result.Message);
        }

        [Theory]
        [MemberData(nameof(GetHasNotConditionData))]
        public async Task HasCondition_False_MustWork(string text, string condition)
        {
            var result = await Semantic.HasConditionAsync(text, condition);
            Assert.False(result.Success);
            Output.WriteLine($"""
                [Explanation]
                {result.Message}
                """);
        }

        public static IEnumerable<object[]> GetHasNotConditionData()
        {
            yield return new object[]
            {
                "Such a beautiful day",
                "talks about night"
            };
            yield return new object[]
            {
                "You fucking bastard",
                "shows kindness"
            };
            yield return new object[]
            {
                "This car is red",
                "I talks about trees"
            };
            yield return new object[]
            {
                "This automobile is red",
                "It talks about blue"
            };
        }

        public static IEnumerable<object[]> GetHasConditionData()
        {
            yield return new object[]
            {
                "Such a beautiful day",
                "talks about a good day"
            };
            yield return new object[]
            {
                "You fucking bastard",
                "shows anger"
            };
            yield return new object[]
            {
                "This car is red",
                "I talks about cars"
            };
            yield return new object[]
            {
                "This car is red",
                "I talks about red"
            };

        }
    }
}