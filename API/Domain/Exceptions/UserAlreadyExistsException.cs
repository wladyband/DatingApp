namespace API.Domain.Exceptions;

/// <summary>
/// Exceção lançada quando um usuário com o email já existe.
/// </summary>
public class UserAlreadyExistsException : DomainException
{
    public string Email { get; }

    public UserAlreadyExistsException(string email)
        : base($"Um usuário com o email '{email}' já existe.")
    {
        Email = email;
    }
}
