using System;
using API.Application.Ports;
using System.Security.Cryptography;
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
            throw new ArgumentException("Email é obrigatório.", nameof(input.Email));

        if (string.IsNullOrWhiteSpace(input.Displayname))
            throw new ArgumentException("Displayname é obrigatório.", nameof(input.Displayname));

        if (string.IsNullOrWhiteSpace(input.Password))
            throw new ArgumentException("Senha é obrigatória.", nameof(input.Password));

        var normalizedEmail = input.Email.Trim();
        var existingUser = await _accountRepository.GetByEmailAsync(normalizedEmail);
        if (existingUser != null)
            throw new InvalidOperationException("Um usuário com este email já existe.");

        using var hmac = new HMACSHA512();
        var computedHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(input.Password));

        var user = new Core.Entities.AppUser
        {
            Email = normalizedEmail,
            Displayname = input.Displayname.Trim(),
            PasswordHash = computedHash,
            PasswordSalt = hmac.Key
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