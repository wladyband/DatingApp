using API.Domain.Entities;

namespace API.Application.Ports.Services;

/// <summary>
/// Port que define como o domínio acessa dados de usuários.
/// Esta é uma abstração que isola o domínio da implementação específica de persistência.
/// </summary>
public interface IUserRepository
{
    /// <summary>
    /// Obtém um usuário por seu ID.
    /// </summary>
    Task<AppUser?> GetByIdAsync(string id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtém um usuário por seu email.
    /// </summary>
    Task<AppUser?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);



    Task AddAsync(AppUser user, CancellationToken cancellationToken = default);

    Task<IEnumerable<AppUser>> GetAllAsync(CancellationToken cancellationToken = default);

    Task UpdateAsync(AppUser user, CancellationToken cancellationToken = default);

    Task RemoveAsync(string id, CancellationToken cancellationToken = default);
}
