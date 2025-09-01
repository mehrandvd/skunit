using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using Xunit;
using skUnit.Scenarios;

namespace skUnit.Tests.AssertionTests
{
    public class ChatScenarioRunnerTests
    {
        [Fact]
        public void ChatScenarioRunner_Constructor_WithLogger_Works()
        {
            // Arrange
            var mockChatClient = CreateMockChatClient();
            var logger = new TestLogger<ChatScenarioRunner>();

            // Act
            var runner = new ChatScenarioRunner(mockChatClient, logger);

            // Assert
            Assert.NotNull(runner);
        }

        [Fact]
        public void ChatScenarioRunner_Constructor_WithActionLog_Works()
        {
            // Arrange
            var mockChatClient = CreateMockChatClient();
            var logs = new List<string>();
            Action<string> onLog = message => logs.Add(message);

            // Act
            var runner = new ChatScenarioRunner(mockChatClient, onLog);

            // Assert
            Assert.NotNull(runner);
        }

        [Fact]
        public void ChatScenarioRunner_Constructor_WithNullLogger_Works()
        {
            // Arrange
            var mockChatClient = CreateMockChatClient();

            // Act
            var runner = new ChatScenarioRunner(mockChatClient, (ILogger<ChatScenarioRunner>?)null);

            // Assert
            Assert.NotNull(runner);
        }

        [Fact]
        public async Task ChatScenarioRunner_RunAsync_ThrowsWhenBothClientAndFuncAreNull()
        {
            // Arrange
            var mockChatClient = CreateMockChatClient();
            var runner = new ChatScenarioRunner(mockChatClient);
            var scenario = new ChatScenario { RawText = "" };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => runner.RunAsync(scenario, (IChatClient?)null, null));

            Assert.Contains("Both chatClient and getAnswerFunc can not be null", exception.Message);
        }

        private static IChatClient CreateMockChatClient()
        {
            return new TestChatClient();
        }

        private class TestChatClient : IChatClient
        {
            public ChatClientMetadata Metadata => new("Test");

            public Task<ChatResponse> GetResponseAsync(IEnumerable<ChatMessage> chatMessages, ChatOptions? options = null, CancellationToken cancellationToken = default)
            {
                return Task.FromResult(new ChatResponse(new ChatMessage(ChatRole.Assistant, "Test response")));
            }

            public async IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(IEnumerable<ChatMessage> chatMessages, ChatOptions? options = null, [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
            {
                await Task.Delay(1, cancellationToken);
                yield break;
            }

            public TService? GetService<TService>(object? key = null) where TService : class => null;
            public object? GetService(Type serviceType, object? key = null) => null;
            public void Dispose() { }
        }

        private class TestLogger<T> : ILogger<T>
        {
            public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
            public bool IsEnabled(LogLevel logLevel) => true;
            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter) { }
        }
    }
}