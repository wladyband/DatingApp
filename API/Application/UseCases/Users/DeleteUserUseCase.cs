using API.Application.Ports.Services;
using API.Domain.Exceptions;

namespace API.Application.UseCases.Users;

public class DeleteUserUseCase
{
    private readonly IUserRepository _userRepository;

    public DeleteUserUseCase(IUserRepository userRepository)
    {
        ArgumentNullException.ThrowIfNull(userRepository);
        _userRepository = userRepository;
    }

    public async Task<bool> DeleteAsync(string userId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw new DomainException("ID do usuário é obrigatório.");

        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (user == null)
            return false;

        await _userRepository.RemoveAsync(userId, cancellationToken);

        return true;
    }
}


