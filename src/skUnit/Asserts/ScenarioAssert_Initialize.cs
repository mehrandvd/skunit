using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using SemanticValidation;
using skUnit.Exceptions;
using skUnit.Scenarios;
using skUnit.Scenarios.Parsers.Assertions;

namespace skUnit
{
    /// <summary>
    /// This class is for testing skUnit scenarios semantically. It contains various methods
    /// that you can test kernels and functions with scenarios. Scenarios are some markdown files with a specific format.
    /// </summary>
    [Obsolete("Use ChatScenarioRunner instead. This class will be removed in a future version.", false)]
    public partial class ScenarioAssert
    {
        private readonly ChatScenarioRunner _runner;

        /// <summary>
        /// This class needs a ChatClient to work.
        /// Pass your pre-configured ChatClient and ILogger to this constructor.
        /// </summary>
        public ScenarioAssert(IChatClient chatClient, ILogger<ScenarioAssert>? logger = null)
        {
            _runner = new ChatScenarioRunner(chatClient, logger != null ? new DelegateLoggerAdapter<ChatScenarioRunner>(msg => logger.LogInformation("{Message}", msg)) : null);
        }

        /// <summary>
        /// This class needs a ChatClient to work.
        /// Pass your pre-configured ChatClient to this constructor.
        /// </summary>
        [Obsolete("Use constructor with ILogger<ScenarioAssert> parameter for better logging integration. This constructor will be deprecated in a future version.")]
        public ScenarioAssert(IChatClient chatClient, Action<string>? onLog)
        {
            _runner = new ChatScenarioRunner(chatClient, onLog);
        }

        /// <summary>
        /// Gets the internal ChatScenarioRunner for advanced usage.
        /// </summary>
        public ChatScenarioRunner Runner => _runner;
    }

    /// <summary>
    /// Internal adapter that wraps an Action&lt;string&gt; delegate to provide ILogger functionality
    /// for backward compatibility with the legacy onLog parameter for ScenarioAssert.
    /// </summary>
    public class DelegateLoggerAdapter : ILogger<ScenarioAssert>
    {
        private readonly Action<string> _logAction;

        public DelegateLoggerAdapter(Action<string> logAction)
        {
            _logAction = logAction ?? throw new ArgumentNullException(nameof(logAction));
        }

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull
        {
            // No scope support needed for this adapter
            return null;
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            // Always enabled for all log levels since the original Action<string> doesn't have level filtering
            return true;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            if (formatter == null) return;

            var message = formatter(state, exception);
            _logAction(message);
        }
    }
}
