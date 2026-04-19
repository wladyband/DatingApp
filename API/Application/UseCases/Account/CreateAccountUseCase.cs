using API.Application.Ports.Persistence;
using API.Core.DomainServices;
using API.Core.Exceptions;

namespace API.Application.UseCases.Account;

public class CreateAccountUseCase
{
    private readonly IAccountRepository _accountRepository;

    public CreateAccountUseCase(IAccountRepository accountRepository)
    {
        _accountRepository = accountRepository;
    }

    public async Task<Core.Entities.AppUser> ExecuteAsync(CreateAccountInput input)
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

        var user = new Core.Entities.AppUser
        {
            Email = normalizedEmail,
            Displayname = input.Displayname.Trim(),
            PasswordHash = computedHash,
            PasswordSalt = salt
        };

        await _accountRepository.AddAsync(user);

        return user;
    }
}

public record CreateAccountInput(
     string Email,
     string Displayname,
     string Password
 );

