using System;
using API.Application.DTOs.Requests.Login;
using API.Application.Ports.Services;
using API.Domain.Services;
using API.Domain.Exceptions;
using API.Application.DTOs.Requests.Users;
using API.Application.Ports.Infrastructure;

namespace API.Application.UseCases.Login;

/// <summary>
/// Use case para realizar login de um usuário.
/// Valida credenciais e retorna a entidade AppUser se autenticado.
/// </summary>
public class LoginUseCase
{
    private readonly IAccountRepository _accountRepository;
    private readonly ITokenService _tokenService;

    public LoginUseCase(IAccountRepository accountRepository, ITokenService tokenService)
    {
        ArgumentNullException.ThrowIfNull(accountRepository);
        ArgumentNullException.ThrowIfNull(tokenService);

        _accountRepository = accountRepository;
        _tokenService = tokenService;
    }

    public async Task<UserInput> LoginAsync(LoginInput input, CancellationToken cancellationToken = default)
    {
        if (input == null)
            throw new ArgumentNullException(nameof(input));

        if (string.IsNullOrWhiteSpace(input.Email))
            throw new DomainException("Email é obrigatório.");

        if (string.IsNullOrWhiteSpace(input.Password))
            throw new DomainException("Senha é obrigatória.");

        var normalizedEmailAddress = input.Email.Trim();

        var user = await _accountRepository.GetByEmailAsync(normalizedEmailAddress, cancellationToken);

        if (user == null)
            throw new InvalidCredentialsException();

        var isPasswordValid = PasswordService.VerifyPassword(
            input.Password,
            user.PasswordHash,
            user.PasswordSalt
        );

        if (!isPasswordValid)
            throw new InvalidCredentialsException();

        return new UserInput
        {
            Id = user.Id,
            Email = user.Email,
            DisplayName = user.DisplayName,
            Token = _tokenService.CreateToken(user)
        };
    }
}
