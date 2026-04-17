using API.Application.Ports;
using API.Core.Entities;

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
        // Validações de negócio podem ser adicionadas aqui
        if (string.IsNullOrWhiteSpace(input.Email))
            throw new ArgumentException("Email é obrigatório.");

        if (string.IsNullOrWhiteSpace(input.Displayname))
            throw new ArgumentException("Nome de exibição é obrigatório.");

        // Verificar se o email já existe
        var existingUser = await _userRepository.GetByEmailAsync(input.Email);
        if (existingUser != null)
            throw new InvalidOperationException("Um usuário com este email já existe.");

        // Criar novo usuário
        var user = new AppUser
        {
            Email = input.Email,
            Displayname = input.Displayname
        };

        // Persistir
        await _userRepository.AddAsync(user);
        await _userRepository.SaveChangesAsync();

        return user;
    }
}

/// <summary>
/// DTO (Data Transfer Object) para entrada do use case.
/// </summary>
public record CreateUserInput(string Email, string Displayname);
