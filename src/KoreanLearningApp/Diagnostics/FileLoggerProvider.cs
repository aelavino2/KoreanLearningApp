using Microsoft.Extensions.Logging;

namespace KoreanLearningApp.Diagnostics;

/// <summary>
/// Пишет логи в файл app-yyyyMMdd.log в <see cref="AppPaths.LogsDirectory"/>, по файлу на день.
/// </summary>
public sealed class FileLoggerProvider(string directory, TimeProvider timeProvider) : ILoggerProvider
{
    private readonly object _writeLock = new();

    public ILogger CreateLogger(string categoryName) => new FileLogger(this, ShortenCategory(categoryName));

    public void Dispose()
    {
    }

    private void Write(LogLevel level, string category, string message, Exception? exception)
    {
        var now = timeProvider.GetLocalNow();
        var line = $"{now:HH:mm:ss.fff} [{level}] {category}: {message}";
        if (exception is not null)
            line += Environment.NewLine + exception;

        var path = Path.Combine(directory, $"app-{now:yyyyMMdd}.log");
        lock (_writeLock)
            File.AppendAllText(path, line + Environment.NewLine);
    }

    private static string ShortenCategory(string categoryName) =>
        categoryName[(categoryName.LastIndexOf('.') + 1)..];

    private sealed class FileLogger(FileLoggerProvider provider, string category) : ILogger
    {
        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

        public bool IsEnabled(LogLevel logLevel) => logLevel >= LogLevel.Information;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            if (IsEnabled(logLevel))
                provider.Write(logLevel, category, formatter(state, exception), exception);
        }
    }
}
