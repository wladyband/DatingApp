using API.Application.Ports;
using API.Application.UseCases.Account;

namespace API.Application.Services;

/// <summary>
/// Application Service para orquestração de operações de conta.
/// Coordena múltiplos use cases e chamadas para ports externos.
/// </summary>
public class AccountApplicationService
{
    private readonly CreateAccountUseCase _createAccountUseCase;
    private readonly IEmailService _emailService;

    public AccountApplicationService(
        CreateAccountUseCase createAccountUseCase,
        IEmailService emailService)
    {
        _createAccountUseCase = createAccountUseCase;
        _emailService = emailService;
    }

    public async Task<Core.Entities.AppUser> CreateAccountAsync(CreateAccountInput input)
    {
        var user = await _createAccountUseCase.ExecuteAsync(input);

        // Orquestração: enviar email de boas-vindas
        await _emailService.SendWelcomeEmailAsync(user.Email, user.Displayname);

        return user;
    }
}
