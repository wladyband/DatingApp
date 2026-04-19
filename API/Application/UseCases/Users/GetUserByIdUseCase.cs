using API.Application.Ports;
using API.Core.Entities;
using API.Core.Exceptions;

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
        _userRepository = userRepository;
    }

    public async Task<AppUser?> ExecuteAsync(string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw new DomainException("ID do usuário é obrigatório.");

        return await _userRepository.GetByIdAsync(userId);
    }
}
