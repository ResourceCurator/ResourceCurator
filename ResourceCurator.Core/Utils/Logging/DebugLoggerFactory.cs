using System;
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using System.Globalization;

namespace Utils.Logging
{
    public sealed class DebugLogger : ILogger
    {
        private readonly string _categoryName;

        public DebugLogger(string categoryName) => _categoryName = categoryName;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception exception,
            Func<TState, Exception, string> formatter)
        {
            Debug.WriteLine($"{DateTime.Now.ToString("o", CultureInfo.InvariantCulture)} {logLevel} {eventId.Id} {_categoryName}");
            if(formatter != null)
                Debug.WriteLine(formatter(state, exception));
        }

        public IDisposable BeginScope<TState>(TState state) => null;
    }

    public sealed class TraceLoggerProvider : ILoggerProvider
    {
        public ILogger CreateLogger(string categoryName) => new DebugLogger(categoryName);

        public void Dispose() { }
    }

}

