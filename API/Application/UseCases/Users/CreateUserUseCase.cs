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
        ArgumentNullException.ThrowIfNull(userRepository);
        ArgumentNullException.ThrowIfNull(emailService);

        _userRepository = userRepository;
        _emailService = emailService;
    }

    public async Task<AppUser> CreateAsync(CreateUserInput input, CancellationToken cancellationToken = default)
    {
        if (input == null)
            throw new ArgumentNullException(nameof(input));

        if (string.IsNullOrWhiteSpace(input.Email))
            throw new DomainException("Email é obrigatório.");

        if (string.IsNullOrWhiteSpace(input.DisplayName))
            throw new DomainException("Nome de exibição é obrigatório.");

        if (string.IsNullOrWhiteSpace(input.Password))
            throw new DomainException("Senha é obrigatória.");

        var normalizedEmailAddress = input.Email.Trim();
        var normalizedDisplayName = input.DisplayName.Trim();

        var existingUser = await _userRepository.GetByEmailAsync(normalizedEmailAddress, cancellationToken);
        if (existingUser != null)
            throw new UserAlreadyExistsException(normalizedEmailAddress);

        var (passwordHash, passwordSalt) = PasswordService.ComputePasswordHash(input.Password);

        var user = new AppUser
        {
            Email = normalizedEmailAddress,
            DisplayName = normalizedDisplayName,
            PasswordHash = passwordHash,
            PasswordSalt = passwordSalt
        };

        await _userRepository.AddAsync(user, cancellationToken);
        await _emailService.SendWelcomeEmailAsync(user.Email, user.DisplayName, cancellationToken);

        return user;
    }
}


