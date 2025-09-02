using skUnit.Scenarios;
using Microsoft.Extensions.AI;
using Xunit;

namespace skUnit.Tests.ReadmeExamples
{
    public class ReadmeExampleTests
    {
        [Fact]
        public void SimpleGreetingExample_ShouldParseCorrectly()
        {
            var markdown = @"# SCENARIO Simple Greeting

## [USER]
Hello!

## [AGENT]
Hi there! How can I help you today?

### CHECK SemanticCondition
It's a friendly greeting response";

            var scenario = ChatScenario.LoadFromText(markdown);

            Assert.Equal(2, scenario.ChatItems.Count);

            // Check first chat item (USER)
            var userChatItem = scenario.ChatItems[0];
            Assert.Equal(ChatRole.User, userChatItem.Role);
            Assert.Equal("Hello!", userChatItem.Content);

            // Check second chat item (AGENT)
            var agentChatItem = scenario.ChatItems[1];
            Assert.Equal(ChatRole.Assistant, agentChatItem.Role);
            Assert.Equal("Hi there! How can I help you today?", agentChatItem.Content);
            Assert.Single(agentChatItem.Assertions);
        }

        [Fact]
        public void JsonCheckExample_ShouldParseCorrectly()
        {
            var markdown = @"# SCENARIO JSON Response

## [USER]
Give me user info as JSON

## [AGENT]
{""name"": ""John"", ""age"": 30, ""city"": ""New York""}

### CHECK JsonCheck
{
  ""name"": [""NotEmpty""],
  ""age"": [""GreaterThan"", 0],
  ""city"": [""SemanticCondition"", ""It's a real city name""]
}";

            var scenario = ChatScenario.LoadFromText(markdown);

            Assert.Equal(2, scenario.ChatItems.Count);

            var agentChatItem = scenario.ChatItems[1];
            Assert.Equal(ChatRole.Assistant, agentChatItem.Role);
            Assert.Single(agentChatItem.Assertions);
        }
    }
}