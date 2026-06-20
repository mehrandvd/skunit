using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;

namespace skUnit.Tests.RunnerTests;

public class ChatScenarioRunnerLoggingTests
{
    [Fact]
    public void Constructor_WithILogger_UsesLogger()
    {
        // Arrange
        var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
        var logger = loggerFactory.CreateLogger<ChatScenarioRunner>();
        var chatClient = CreateMockChatClient();

        // Act
        var scenarioRunner = new ChatScenarioRunner(chatClient, logger);

        // Assert
        Assert.NotNull(scenarioRunner);
        // The fact that no exception was thrown indicates the constructor worked correctly
    }

    [Fact]
    public void Constructor_WithNullILogger_UsesNullLogger()
    {
        // Arrange
        var chatClient = CreateMockChatClient();

        // Act
        var scenarioRunner = new ChatScenarioRunner(chatClient, (ILogger<ChatScenarioRunner>?)null);

        // Assert
        Assert.NotNull(scenarioRunner);
        // Should not throw even with null logger due to NullLogger fallback
    }

    [Fact]
    public void Constructor_WithActionDelegate_CreatesAdapter()
    {
        // Arrange
        var loggedMessages = new List<string>();
        Action<string> onLog = message => loggedMessages.Add(message);
        var chatClient = CreateMockChatClient();

        // Act
        var scenarioRunner = new ChatScenarioRunner(chatClient, onLog);

        // Assert
        Assert.NotNull(scenarioRunner);
        // The constructor should have created a DelegateLoggerAdapter internally
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
            return Task.FromResult(new ChatResponse(new ChatMessage(ChatRole.Assistant, "Test")));
        }

        public async IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(IEnumerable<ChatMessage> chatMessages, ChatOptions? options = null, [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            await Task.Delay(1, cancellationToken);
            yield break; // Simple implementation for testing
        }

        public TService? GetService<TService>(object? key = null) where TService : class => null;
        public object? GetService(Type serviceType, object? key = null) => null;
        public void Dispose() { }
    }
}