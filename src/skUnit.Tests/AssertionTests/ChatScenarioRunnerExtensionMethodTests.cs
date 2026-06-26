using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Xunit;
using skUnit.Scenarios;
using skUnit.Scenarios.Parsers.Assertions;
using System.Text.Json;
using skUnit.Runners;

namespace skUnit.Tests.AssertionTests
{
    public class ChatScenarioRunnerExtensionMethodTests
    {
        [Fact]
        public async Task IChatClient_ExecuteScenarioAsync_UsesClientAsAssertionClientByDefault()
        {
            var chatClient = CreateMockChatClient("Hello there");
            var scenario = CreateScenario("IChatClient extension", "Hello there");

            await chatClient.ExecuteScenarioAsync(scenario);
        }

        [Fact]
        public async Task AIAgent_ExecuteScenarioAsync_RequiresAssertionClient()
        {
            var assertionClient = CreateMockChatClient("Hello there");
            var agent = new TestAIAgent("Hello there");
            var scenario = CreateScenario("AIAgent extension", "Hello there");

            await agent.ExecuteScenarioAsync(scenario, assertionClient);
        }

        [Fact]
        public async Task IEnumerableChatScenario_ExecuteScenarioAsync_WithAgent_Works()
        {
            var assertionClient = CreateMockChatClient("Hello there");
            var agent = new TestAIAgent("Hello there");
            var scenario = CreateScenario("IEnumerable extension", "Hello there");

            await agent.ExecuteScenarioAsync(new[] { scenario }, assertionClient);
        }

        [Fact]
        public async Task ChatScenarioRunner_RunAsync_WithChatClient_Works()
        {
            var chatClient = CreateMockChatClient("Hello there");
            var scenario = CreateScenario("ChatScenario extension", "Hello there");
            var runner = new ChatScenarioRunner(chatClient);

            await runner.RunAsync(scenario, chatClient);
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
    }
}
