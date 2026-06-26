using Microsoft.Extensions.Logging;

namespace skUnit
{
    /// <summary>
    /// Internal adapter that wraps an Action&lt;string&gt; delegate to provide ILogger functionality
    /// for backward compatibility with the legacy onLog parameter.
    /// </summary>
    public class DelegateLoggerAdapter : ILogger
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