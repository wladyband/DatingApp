using API.Application.Ports.External;

namespace API.Infrastructure.Services;

/// <summary>
/// Implementação de logger que adapta Microsoft.Extensions.Logging para port customizado.
/// </summary>
public class LoggerPortAdapter : ILoggerPort
{
    private readonly ILogger _logger;

    public LoggerPortAdapter(ILoggerFactory loggerFactory)
    {
        _logger = loggerFactory.CreateLogger("Application");
    }

    public void LogInformation(string message, params object[] args) =>
        _logger.LogInformation(message, args);

    public void LogWarning(string message, params object[] args) =>
        _logger.LogWarning(message, args);

    public void LogError(string message, Exception? exception = null, params object[] args) =>
        _logger.LogError(exception, message, args);

    public void LogDebug(string message, params object[] args) =>
        _logger.LogDebug(message, args);
}


