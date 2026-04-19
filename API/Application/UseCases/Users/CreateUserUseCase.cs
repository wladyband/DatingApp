using API.Application.Ports.Persistence;
using API.Core.Entities;
using API.Core.DomainServices;
using API.Core.Exceptions;

namespace API.Application.UseCases.Users;

/// <summary>
/// Use Case: Criar um novo usuário.
/// Este é um exemplo de como a lógica de negócio será isolada em casos de uso.
/// </summary>
public class CreateUserUseCase
{
    private readonly IUserRepository _userRepository;

    public CreateUserUseCase(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<AppUser> ExecuteAsync(CreateUserInput input)
    {
        if (string.IsNullOrWhiteSpace(input.Email))
            throw new DomainException("Email é obrigatório.");

        if (string.IsNullOrWhiteSpace(input.Displayname))
            throw new DomainException("Nome de exibição é obrigatório.");

        if (string.IsNullOrWhiteSpace(input.Password))
            throw new DomainException("Senha é obrigatória.");

        var existingUser = await _userRepository.GetByEmailAsync(input.Email);
        if (existingUser != null)
            throw new UserAlreadyExistsException(input.Email);

        var (passwordHash, passwordSalt) = PasswordService.ComputePasswordHash(input.Password);

        var user = new AppUser
        {
            Email = input.Email,
            Displayname = input.Displayname,
            PasswordHash = passwordHash,
            PasswordSalt = passwordSalt
        };

        await _userRepository.AddAsync(user);

        return user;
    }
}

/// <summary>
/// DTO (Data Transfer Object) para entrada do use case.
/// </summary>
public record CreateUserInput(string Email, string Displayname, string Password);


