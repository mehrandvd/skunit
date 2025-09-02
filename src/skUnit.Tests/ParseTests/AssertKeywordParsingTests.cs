using Microsoft.Extensions.AI;
using skUnit.Scenarios;
using skUnit.Scenarios.Parsers;
using Xunit;

namespace skUnit.Tests.ParseTests
{
    public class AssertKeywordParsingTests
    {
        [Fact]
        public void ChatScenarioParser_ParsesAssertKeyword()
        {
            // Arrange
            const string scenarioText = """
            # SCENARIO Simple Assert Test

            ## [USER]
            Hello world

            ## [ASSISTANT]
            Hi there!

            ### ASSERT SemanticCondition
            It's a greeting response
            """;

            var parser = new ChatScenarioParser();

            // Act
            var scenario = parser.Parse(scenarioText);

            // Assert
            Assert.Equal("Simple Assert Test", scenario.Description);
            Assert.Equal(2, scenario.ChatItems.Count);

            var assistantItem = scenario.ChatItems.First(x => x.Role == ChatRole.Assistant);
            Assert.Single(assistantItem.Assertions);
            Assert.Equal("Condition", assistantItem.Assertions[0].AssertionType);
        }

        [Fact]
        public void ChatScenarioParser_ParsesBothCheckAndAssertKeywords()
        {
            // Arrange
            const string scenarioText = """
            # SCENARIO Check and Assert Test

            ## [USER]
            Tell me about cats

            ## [ASSISTANT]
            Cats are wonderful pets.

            ### CHECK ContainsAll
            cats, pets

            ### ASSERT SemanticCondition
            It mentions pets positively
            """;

            var parser = new ChatScenarioParser();

            // Act
            var scenario = parser.Parse(scenarioText);

            // Assert
            var assistantItem = scenario.ChatItems.First(x => x.Role == ChatRole.Assistant);
            Assert.Equal(2, assistantItem.Assertions.Count);

            Assert.Equal("ContainsAll", assistantItem.Assertions[0].AssertionType);
            Assert.Equal("Condition", assistantItem.Assertions[1].AssertionType);
        }


    }
}