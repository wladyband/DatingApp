namespace API.Application.Ports;

/// <summary>
/// Port para serviço de email. Permite enviar emails sem acoplamento a provedores específicos.
/// </summary>
public interface IEmailService
{
    Task SendWelcomeEmailAsync(string email, string displayName);
    Task SendPasswordResetEmailAsync(string email, string resetToken);
}
