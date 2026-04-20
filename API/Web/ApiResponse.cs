namespace API.Web;

/// <summary>
/// Envelope padrão para todas as respostas HTTP da API.
/// Garante consistência e facilita tratamento de erros no frontend.
/// </summary>
public record ApiResponse<T>(
    bool Success,
    T? Data,
    string? ErrorMessage,
    string? ErrorCode = null,
    object? Details = null
)
{
    /// <summary>
    /// Cria resposta de sucesso.
    /// </summary>
    public static ApiResponse<T> SuccessResponse(T data) =>
        new(Success: true, Data: data, ErrorMessage: null);

    /// <summary>
    /// Cria resposta de erro.
    /// </summary>
    public static ApiResponse<T> ErrorResponse(string message, string? code = null, object? details = null) =>
        new(Success: false, Data: default, ErrorMessage: message, ErrorCode: code, Details: details);
}

/// <summary>
/// Variação genérica para respostas sem dados (ex: delete).
/// </summary>
public record ApiResponse(
    bool Success,
    string? ErrorMessage,
    string? ErrorCode = null,
    object? Details = null
)
{
    /// <summary>
    /// Cria resposta de sucesso sem dados.
    /// </summary>
    public static ApiResponse SuccessResponse() =>
        new(Success: true, ErrorMessage: null);

    /// <summary>
    /// Cria resposta de erro.
    /// </summary>
    public static ApiResponse ErrorResponse(string message, string? code = null, object? details = null) =>
        new(Success: false, ErrorMessage: message, ErrorCode: code, Details: details);
}
