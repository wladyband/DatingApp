using API.Application.DTOs.Requests.Account;
using API.Application.Ports.External;
using API.Application.Ports.Services;
using API.Domain.Services;
using API.Domain.Entities;
using API.Domain.Exceptions;

namespace API.Application.UseCases.Account;

public class CreateAccountUseCase
{
    private readonly IAccountRepository _accountRepository;
    private readonly IEmailPortAdapter _emailService;

    public CreateAccountUseCase(IAccountRepository accountRepository, IEmailPortAdapter emailService)
    {
        _accountRepository = accountRepository;
        _emailService = emailService;
    }

    public async Task<AppUser> ExecuteAsync(CreateAccountInput input)
    {
        if (input == null)
            throw new ArgumentNullException(nameof(input));

        if (string.IsNullOrWhiteSpace(input.Email))
            throw new DomainException("Email é obrigatório.");

        if (string.IsNullOrWhiteSpace(input.Displayname))
            throw new DomainException("Displayname é obrigatório.");

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
            Displayname = input.Displayname.Trim(),
            PasswordHash = computedHash,
            PasswordSalt = salt
        };

        await _accountRepository.AddAsync(user);
        await _emailService.SendWelcomeEmailAsync(user.Email, user.Displayname);

        return user;
    }
}

