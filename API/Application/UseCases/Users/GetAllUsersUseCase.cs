using API.Application.Ports;
using API.Core.Entities;

namespace API.Application.UseCases.Users;

public class GetAllUsersUseCase
{
    private readonly IUserRepository _userRepository;

    public GetAllUsersUseCase(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<IEnumerable<AppUser>> ExecuteAsync()
    {
        return await _userRepository.GetAllAsync();
    }
}
