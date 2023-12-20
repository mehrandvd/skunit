using skUnit.Exceptions;
using Xunit.Abstractions;

namespace skUnit.Tests
{
    public class SemanticAssertTests
    {
        private ITestOutputHelper Output { get; set; }
        public SemanticAssertTests(ITestOutputHelper output)
        {
            Output = output;

            var apiKey =
                Environment.GetEnvironmentVariable("openai-api-key", EnvironmentVariableTarget.User) ??
                throw new Exception("No ApiKey in environment variables.");
            var endpoint =
                Environment.GetEnvironmentVariable("openai-endpoint", EnvironmentVariableTarget.User) ??
                throw new Exception("No Endpoint in environment variables.");
            
            SemanticAssert.Initialize(endpoint, apiKey);
        }

        [Theory]
        [MemberData(nameof(GetSameData))]
        public void AreSame_True_MustWork(string first, string second)
        {
            SemanticAssert.AreSame(first, second);
        }

        [Theory]
        [MemberData(nameof(GetNotSameData))]
        public void AreSame_False_MustWork(string first, string second)
        {
            var exception = Assert.Throws<SemanticAssertException>(() => SemanticAssert.AreSame(first, second));
            Output.WriteLine($"""
                [Explanation]
                {exception.Message}
                """);
        }

        [Theory]
        [MemberData(nameof(GetNotSameData))]
        public void AreNotSame_True_MustWork(string first, string second)
        {
            SemanticAssert.AreNotSame(first, second);
        }

        [Theory]
        [MemberData(nameof(GetSameData))]
        public void AreNotSame_False_MustWork(string first, string second)
        {
            var exception = Assert.Throws<SemanticAssertException>(() => SemanticAssert.AreNotSame(first, second));
            Output.WriteLine($"""
                [Explanation]
                {exception.Message}
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