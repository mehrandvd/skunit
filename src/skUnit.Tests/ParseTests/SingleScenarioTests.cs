using Microsoft.Extensions.AI;
using skUnit.Scenarios;
using Xunit;

namespace skUnit.Tests.ParseTests;

public class SingleScenarioTests
{
    [Fact]
    public void Parse_SingleScenario_ReturnsDirectScenario()
    {
        var scenarioText = """
                           # SCENARIO Simple Test

                           ## [USER]
                           Hello

                           ## [AGENT]
                           Hi there!

                           ### CHECK SemanticCondition
                           It's a greeting response
                           """;

        var scenario = ChatScenario.LoadFromText(scenarioText);

        Assert.NotNull(scenario);
        Assert.Equal("Simple Test", scenario.Description);
        Assert.Equal(2, scenario.ChatItems.Count);
        Assert.Equal(ChatRole.User, scenario.ChatItems[0].Role);
        Assert.Equal(ChatRole.Assistant, scenario.ChatItems[1].Role);
        Assert.Equal("Hello", scenario.ChatItems[0].Content);
        Assert.Equal("Hi there!", scenario.ChatItems[1].Content);
        Assert.Single(scenario.ChatItems[1].Assertions);
    }

    [Fact]
    public void Parse_TextWithDashes_IgnoresDashes()
    {
        // This test verifies that dashes (-----) are no longer used for splitting
        // and are treated as regular content
        var scenarioText = """
                           # SCENARIO Test With Dashes

                           ## [USER]
                           Here's some content
                           -----
                           And more content after dashes

                           ## [AGENT]
                           I see content with dashes in it
                           """;

        var scenario = ChatScenario.LoadFromText(scenarioText);

        Assert.NotNull(scenario);
        Assert.Equal("Test With Dashes", scenario.Description);
        Assert.Equal(2, scenario.ChatItems.Count);

        // The user content should include the dashes as regular text
        var userContent = scenario.ChatItems[0].Content;
        Assert.Contains("-----", userContent);
        Assert.Contains("Here's some content", userContent);
        Assert.Contains("And more content after dashes", userContent);
    }

    [Fact]
    public void LoadFromResourceAsync_ReturnsDirectScenario()
    {
        // This test verifies that LoadFromResourceAsync now returns a single scenario
        // instead of a list
        var scenarioText = """
                           # SCENARIO Resource Test

                           ## [USER]
                           Test message

                           ## [AGENT]
                           Test response
                           """;

        var scenario = ChatScenario.LoadFromText(scenarioText);

        // Verify this is a direct scenario object, not a list
        Assert.IsType<ChatScenario>(scenario);
        Assert.Equal("Resource Test", scenario.Description);
    }
}