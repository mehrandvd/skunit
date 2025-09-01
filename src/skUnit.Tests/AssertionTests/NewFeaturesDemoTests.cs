using Microsoft.Extensions.AI;
using skUnit.Scenarios;
using skUnit.Scenarios.Parsers;
using Xunit;

namespace skUnit.Tests.AssertionTests
{
    public class NewFeaturesDemoTests
    {
        [Fact]
        public void ChatScenarioParser_CanParseExampleScenario()
        {
            // Arrange
            const string scenarioText = """
            # SCENARIO Simple Greeting Test

            ## [USER]
            Hello, how are you today?

            ## [ASSISTANT]
            Hello! I'm doing well, thank you for asking. How can I help you today?

            ### ASSERT Condition
            The response is a polite greeting

            ### ASSERT ContainsText
            well, help

            ### ASSERT Condition
            It mentions a helpful tone
            """;

            var parser = new ChatScenarioParser();

            // Act
            var scenarios = parser.Parse(scenarioText);

            // Assert
            Assert.Single(scenarios);
            Assert.Equal("Simple Greeting Test", scenarios[0].Description);

            var assistantItem = scenarios[0].ChatItems.First(x => x.Role == ChatRole.Assistant);
            Assert.Equal(3, assistantItem.Assertions.Count);

            // Verify different assertion types work
            Assert.Equal("Condition", assistantItem.Assertions[0].AssertionType);
            Assert.Equal("ContainsAll", assistantItem.Assertions[1].AssertionType);
            Assert.Equal("Condition", assistantItem.Assertions[2].AssertionType);
        }

        [Fact]
        public void ChatScenarioRunner_ConstructorWorksAsExpected()
        {
            // Arrange
            var mockChatClient = new TestChatClient();

            // Act & Assert - Should not throw
            var runner = new ChatScenarioRunner(mockChatClient);
            Assert.NotNull(runner);
        }

        private class TestChatClient : IChatClient
        {
            public ChatClientMetadata Metadata => new("Test");
            public Task<ChatResponse> GetResponseAsync(IEnumerable<ChatMessage> chatMessages, ChatOptions? options = null, CancellationToken cancellationToken = default)
                => Task.FromResult(new ChatResponse(new ChatMessage(ChatRole.Assistant, "Test response")));
            public async IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(IEnumerable<ChatMessage> chatMessages, ChatOptions? options = null, [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
            {
                await Task.Delay(1, cancellationToken);
                yield break;
            }
            public TService? GetService<TService>(object? key = null) where TService : class => null;
            public object? GetService(Type serviceType, object? key = null) => null;
            public void Dispose() { }
        }
    }
}