using API.Application.DTOs.Requests.Users;
using API.Application.Ports.External;
using API.Application.Ports.Services;
using API.Domain.Entities;
using API.Domain.Services;
using API.Domain.Exceptions;

namespace API.Application.UseCases.Users;

/// <summary>
/// Use Case: Criar um novo usuário.
/// Este é um exemplo de como a lógica de negócio será isolada em casos de uso.
/// </summary>
public class CreateUserUseCase
{
    private readonly IUserRepository _userRepository;
    private readonly IEmailPortAdapter _emailService;

    public CreateUserUseCase(IUserRepository userRepository, IEmailPortAdapter emailService)
    {
        _userRepository = userRepository;
        _emailService = emailService;
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
        await _emailService.SendWelcomeEmailAsync(user.Email, user.Displayname);

        return user;
    }
}


