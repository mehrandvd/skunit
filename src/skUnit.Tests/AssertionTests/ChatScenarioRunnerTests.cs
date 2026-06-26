using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using Xunit;
using skUnit.Scenarios;
using skUnit.Scenarios.Parsers.Assertions;
using System.Text.Json;

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
        public void ChatScenarioRunner_Constructor_WithObsoleteActionLog_Works()
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
        public async Task ChatScenarioRunner_RunAsync_WithGetAnswerFunc_Works()
        {
            var runner = new ChatScenarioRunner(CreateMockChatClient());
            var scenario = new ChatScenario
            {
                Description = "GetAnswerFunc scenario",
                RawText = "GetAnswerFunc scenario",
                ChatItems =
                [
                    new ChatItem(ChatRole.User, "Hello"),
                    new ChatItem(ChatRole.Assistant, "Hello there")
                ]
            };

            var sawUserMessage = false;

            await runner.RunAsync(
                scenario,
                (history, cancellationToken) =>
                {
                    sawUserMessage = history.Count == 1 && history[0].Role == ChatRole.User && history[0].Text == "Hello";
                    return ValueTask.FromResult(new ChatResponse(new ChatMessage(ChatRole.Assistant, "Hello there")));
                });

            Assert.True(sawUserMessage);
        }

        [Fact]
        public async Task ChatScenarioRunner_RunAsync_WithAIAgentSystemUnderTest_Works()
        {
            var runner = new ChatScenarioRunner(CreateMockChatClient());
            var scenario = new ChatScenario
            {
                Description = "AIAgent scenario",
                RawText = "AIAgent scenario",
                ChatItems =
                [
                    new ChatItem(ChatRole.User, "Hello"),
                    new ChatItem(ChatRole.Assistant, "Hello there")
                ]
            };

            await runner.RunAsync(scenario, new TestAIAgent("Hello there"));
        }

        private static ChatScenario CreateScenario(string description, string expectedResponse)
        {
            return new ChatScenario
            {
                Description = description,
                RawText = description,
                ChatItems =
                [
                    new ChatItem(ChatRole.User, "Hello"),
                    new ChatItem(ChatRole.Assistant, "Hello there")
                    {
                        Assertions = [new EqualsAssertion { ExpectedAnswer = expectedResponse }]
                    }
                ]
            };
        }

        private static IChatClient CreateMockChatClient(string? responseText = null)
        {
            return new TestChatClient(responseText ?? "Test response");
        }

        private class TestChatClient(string responseText) : IChatClient
        {
            public ChatClientMetadata Metadata => new("Test");

            public Task<ChatResponse> GetResponseAsync(IEnumerable<ChatMessage> chatMessages, ChatOptions? options = null, CancellationToken cancellationToken = default)
            {
                return Task.FromResult(new ChatResponse(new ChatMessage(ChatRole.Assistant, responseText)));
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

        private class TestAIAgent : AIAgent
        {
            private readonly string _responseText;

            public TestAIAgent(string responseText = "Test response")
            {
                _responseText = responseText;
            }

            protected override Task<AgentResponse> RunCoreAsync(IEnumerable<ChatMessage> messages, AgentSession? session, AgentRunOptions? options, CancellationToken cancellationToken)
            {
                return Task.FromResult(new AgentResponse(new ChatMessage(ChatRole.Assistant, _responseText)));
            }

            protected override async IAsyncEnumerable<AgentResponseUpdate> RunCoreStreamingAsync(IEnumerable<ChatMessage> messages, AgentSession? session, AgentRunOptions? options, [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken)
            {
                await Task.CompletedTask;
                yield break;
            }

            protected override ValueTask<AgentSession> CreateSessionCoreAsync(CancellationToken cancellationToken)
            {
                return ValueTask.FromResult<AgentSession>(new TestAgentSession());
            }

            protected override ValueTask<JsonElement> SerializeSessionCoreAsync(AgentSession session, JsonSerializerOptions? options, CancellationToken cancellationToken)
            {
                return ValueTask.FromResult(JsonSerializer.SerializeToElement(new { }));
            }

            protected override ValueTask<AgentSession> DeserializeSessionCoreAsync(JsonElement document, JsonSerializerOptions? options, CancellationToken cancellationToken)
            {
                return ValueTask.FromResult<AgentSession>(new TestAgentSession());
            }

            public override string Name => "TestAgent";
            public override string Description => "Test agent";
            protected override string IdCore => "test-agent";
        }

        private sealed class TestAgentSession : AgentSession
        {
            public TestAgentSession()
                : base(new AgentSessionStateBag())
            {
            }
        }

        private class TestLogger<T> : ILogger<T>
        {
            public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
            public bool IsEnabled(LogLevel logLevel) => true;
            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter) { }
        }
    }
}