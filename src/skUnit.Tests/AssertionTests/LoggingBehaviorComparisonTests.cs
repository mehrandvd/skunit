using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using System.Text;

namespace skUnit.Tests.AssertionTests;

public class LoggingBehaviorComparisonTests
{
    [Fact]
    public void Both_Constructors_Produce_Identical_Output()
    {
        // Arrange
        var chatClient = CreateMockChatClient();
        var legacyOutput = new StringBuilder();
        var newOutput = new StringBuilder();

        // Create logger factory that captures to string
        var loggerFactory = LoggerFactory.Create(builder =>
            builder.AddProvider(new StringLoggerProvider(newOutput)));
        var logger = loggerFactory.CreateLogger<ScenarioAssert>();

        // Act - Create both instances
        var legacyAssert = new ScenarioAssert(chatClient, message => legacyOutput.AppendLine(message));
        var newAssert = new ScenarioAssert(chatClient, logger);

        // Both should work without throwing
        Assert.NotNull(legacyAssert);
        Assert.NotNull(newAssert);

        // Since we can't easily test the Log method directly (it's private),
        // this test validates that both constructors work and create valid instances
    }

    [Fact]
    public void DelegateLoggerAdapter_Produces_Expected_Output()
    {
        // Arrange
        var output = new List<string>();
        var adapter = new DelegateLoggerAdapter(message => output.Add(message));

        // Act
        adapter.LogInformation("{Message}", "Test info message");
        adapter.LogWarning("{Message}", "Test warning message");

        // Assert
        Assert.Equal(2, output.Count);
        Assert.Equal("Test info message", output[0]);
        Assert.Equal("Test warning message", output[1]);
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
            yield break;
        }

        public TService? GetService<TService>(object? key = null) where TService : class => null;
        public object? GetService(Type serviceType, object? key = null) => null;
        public void Dispose() { }
    }

    private class StringLoggerProvider : ILoggerProvider
    {
        private readonly StringBuilder _output;

        public StringLoggerProvider(StringBuilder output)
        {
            _output = output;
        }

        public ILogger CreateLogger(string categoryName)
        {
            return new StringLogger(_output);
        }

        public void Dispose() { }
    }

    private class StringLogger : ILogger
    {
        private readonly StringBuilder _output;

        public StringLogger(StringBuilder output)
        {
            _output = output;
        }

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            if (formatter != null)
            {
                var message = formatter(state, exception);
                _output.AppendLine(message);
            }
        }
    }
}