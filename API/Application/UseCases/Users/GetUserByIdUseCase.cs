using API.Application.Ports.Services;
using API.Domain.Entities;
using API.Domain.Exceptions;

namespace API.Application.UseCases.Users;

/// <summary>
/// Use Case: Obter um usuário pelo ID.
/// Exemplo adicional mostrando o padrão de use cases.
/// </summary>
public class GetUserByIdUseCase
{
    private readonly IUserRepository _userRepository;

    public GetUserByIdUseCase(IUserRepository userRepository)
    {
        ArgumentNullException.ThrowIfNull(userRepository);
        _userRepository = userRepository;
    }

    public async Task<AppUser?> GetByIdAsync(string userId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw new DomainException("ID do usuário é obrigatório.");

        return await _userRepository.GetByIdAsync(userId, cancellationToken);
    }
}


