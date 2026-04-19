namespace API.Application.Ports.External;

/// <summary>
/// Port para serviço de logging. Desacopla Application de implementações técnicas de logging.
/// </summary>
public interface ILoggerPort
{
    void LogInformation(string message, params object[] args);
    void LogWarning(string message, params object[] args);
    void LogError(string message, Exception? exception = null, params object[] args);
    void LogDebug(string message, params object[] args);
}


