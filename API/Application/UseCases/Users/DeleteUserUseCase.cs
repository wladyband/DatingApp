using API.Application.Ports.Persistence;
using API.Domain.Exceptions;

namespace API.Application.UseCases.Users;

public class DeleteUserUseCase
{
    private readonly IUserRepository _userRepository;

    public DeleteUserUseCase(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<bool> ExecuteAsync(string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw new DomainException("ID do usuário é obrigatório.");

        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
            return false;

        await _userRepository.RemoveAsync(userId);

        return true;
    }
}


