using API.Application.DTOs.Requests.Account;
using API.Application.Ports.External;
using API.Application.Ports.Services;
using API.Domain.Services;
using API.Domain.Entities;
using API.Domain.Exceptions;
using API.Application.DTOs.Requests.Users;
using API.Application.Ports.Infrastructure;

namespace API.Application.UseCases.Account;

public class CreateAccountUseCase
{
    private readonly IAccountRepository _accountRepository;
    private readonly IEmailPortAdapter _emailService;
    private readonly ITokenService _tokenService;

    public CreateAccountUseCase(IAccountRepository accountRepository, IEmailPortAdapter emailService, ITokenService tokenService)
    {
        _accountRepository = accountRepository;
        _emailService = emailService;
        _tokenService = tokenService;
    }

    public async Task<UserInput> RegisterAsync(CreateAccountInput input)
    {
        if (input == null)
            throw new ArgumentNullException(nameof(input));

        if (string.IsNullOrWhiteSpace(input.Email))
            throw new DomainException("Email é obrigatório.");

        if (string.IsNullOrWhiteSpace(input.DisplayName))
            throw new DomainException("DisplayName é obrigatório.");

        if (string.IsNullOrWhiteSpace(input.Password))
            throw new DomainException("Senha é obrigatória.");

        var normalizedEmail = input.Email.Trim();

        var existingUser = await _accountRepository.GetByEmailAsync(normalizedEmail);
        if (existingUser != null)
            throw new UserAlreadyExistsException(normalizedEmail);

        var (computedHash, salt) = PasswordService.ComputePasswordHash(input.Password);

        var user = new AppUser
        {
            Email = normalizedEmail,
            DisplayName = input.DisplayName.Trim(),
            PasswordHash = computedHash,
            PasswordSalt = salt
        };

        await _accountRepository.AddAsync(user);
        await _emailService.SendWelcomeEmailAsync(user.Email, user.DisplayName);

        return new UserInput
        {
            Id = user.Id,
            Email = user.Email,
            DisplayName = user.DisplayName,
            Token = _tokenService.CreateToken(user)
        };
    }
}

