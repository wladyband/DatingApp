namespace API.Domain.Exceptions;

/// <summary>
/// Exceção lançada quando as credenciais de usuário são inválidas.
/// </summary>
public class InvalidCredentialsException : DomainException
{
    /// <summary>
    /// Email que foi tentado no login.
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// Tipo de erro: "EMAIL_NOT_FOUND" ou "INVALID_PASSWORD".
    /// </summary>
    public string? ErrorType { get; set; }

    /// <summary>
    /// Construtor com mensagem padrão genérica (por segurança).
    /// </summary>
    public InvalidCredentialsException(string message = "Email ou senha inválidos.")
        : base(message) { }

    /// <summary>
    /// Construtor com email e tipo de erro para logging interno.
    /// </summary>
    public InvalidCredentialsException(string email, string errorType)
        : base("Email ou senha inválidos.")
    {
        Email = email;
        ErrorType = errorType;
    }

    /// <summary>
    /// Construtor completo com mensagem customizada.
    /// </summary>
    public InvalidCredentialsException(string message, string email, string errorType)
        : base(message)
    {
        Email = email;
        ErrorType = errorType;
    }
}
