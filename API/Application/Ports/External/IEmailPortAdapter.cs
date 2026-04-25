namespace API.Application.Ports.External;

/// <summary>
/// Port para serviço de email. Permite enviar emails sem acoplamento a provedores específicos.
/// </summary>
public interface IEmailPortAdapter
{
    Task SendWelcomeEmailAsync(string email, string displayName, CancellationToken cancellationToken = default);
    Task SendPasswordResetEmailAsync(string email, string resetToken, CancellationToken cancellationToken = default);
}


