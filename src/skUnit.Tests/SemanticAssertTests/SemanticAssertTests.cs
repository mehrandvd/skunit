using Azure.AI.OpenAI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using skUnit.Exceptions;
using skUnit.Tests.Infrastructure;
using Xunit.Abstractions;

namespace skUnit.Tests.SemanticAssertTests
{
    public class SemanticAssertTests(ITestOutputHelper output) : SemanticTestBase(output)
    {
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