namespace API.Domain.Exceptions;

/// <summary>
/// Exceção lançada quando as credenciais de usuário são inválidas.
/// </summary>
public class InvalidCredentialsException : DomainException
{
    public InvalidCredentialsException(string message = "Email ou senha inválidos.")
        : base(message) { }
}
