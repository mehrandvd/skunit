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
            var scenarios = parser.Parse(scenarioText);

            // Assert
            Assert.Single(scenarios);
            Assert.Equal("Simple Assert Test", scenarios[0].Description);
            Assert.Equal(2, scenarios[0].ChatItems.Count);

            var assistantItem = scenarios[0].ChatItems.First(x => x.Role == ChatRole.Assistant);
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
            var scenarios = parser.Parse(scenarioText);

            // Assert
            Assert.Single(scenarios);
            var assistantItem = scenarios[0].ChatItems.First(x => x.Role == ChatRole.Assistant);
            Assert.Equal(2, assistantItem.Assertions.Count);

            Assert.Equal("ContainsAll", assistantItem.Assertions[0].AssertionType);
            Assert.Equal("Condition", assistantItem.Assertions[1].AssertionType);
        }

        [Fact]
        public void KernelAssertionParser_ParsesSynonyms()
        {
            // Test JsonStructure synonym
            var jsonAssertion = KernelAssertionParser.Parse("{\"test\": \"value\"}", "JsonStructure");
            Assert.Equal("JsonCheck", jsonAssertion.AssertionType);

            // Test Condition synonym
            var conditionAssertion = KernelAssertionParser.Parse("It should be positive", "Condition");
            Assert.Equal("Condition", conditionAssertion.AssertionType);

            // Test Similar synonym
            var similarAssertion = KernelAssertionParser.Parse("Test response", "Similar");
            Assert.Equal("SemanticSimilar", similarAssertion.AssertionType);

            // Test ContainsText synonym
            var containsAssertion = KernelAssertionParser.Parse("test,value", "ContainsText");
            Assert.Equal("ContainsAll", containsAssertion.AssertionType);

            // Test ToolCall synonym
            var toolCallAssertion = KernelAssertionParser.Parse("{\"function\": \"test\"}", "ToolCall");
            Assert.Equal("FunctionCall", toolCallAssertion.AssertionType);
        }

        [Fact]
        public void ChatScenarioParser_ParsesAssertWithSynonyms()
        {
            // Arrange
            const string scenarioText = """
            # SCENARIO Synonym Test

            ## [USER]
            Create a JSON response

            ## [ASSISTANT]
            {"name": "John", "age": 30}

            ### ASSERT JsonStructure
            {
              "name": ["NotEmpty"],
              "age": ["Equal", 30]
            }
            """;

            var parser = new ChatScenarioParser();

            // Act
            var scenarios = parser.Parse(scenarioText);

            // Assert
            Assert.Single(scenarios);
            var assistantItem = scenarios[0].ChatItems.First(x => x.Role == ChatRole.Assistant);
            Assert.Single(assistantItem.Assertions);
            Assert.Equal("JsonCheck", assistantItem.Assertions[0].AssertionType);
        }
    }
}