using API.Application.Ports.External;

namespace API.Infrastructure.External;

/// <summary>
/// Implementação simples de email (placeholder).
/// Em produção, integrar com SendGrid, AWS SES, ou similar.
/// </summary>
public class EmailPortAdapter : IEmailPortAdapter
{
    private readonly ILogger<EmailPortAdapter> _logger;

    public EmailPortAdapter(ILogger<EmailPortAdapter> logger)
    {
        _logger = logger;
    }

    public Task SendWelcomeEmailAsync(string email, string displayName, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Email de boas-vindas seria enviado para {Email} ({DisplayName})",
            email, displayName);

        return Task.CompletedTask;
    }

    public Task SendPasswordResetEmailAsync(string email, string resetToken, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Email de reset de senha seria enviado para {Email}",
            email);

        return Task.CompletedTask;
    }
}


