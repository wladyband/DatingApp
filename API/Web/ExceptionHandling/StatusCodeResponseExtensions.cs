using System.Text.Json;

namespace API.Web.ExceptionHandling;

/// <summary>
/// Gera payload padrao de erro para respostas HTTP sem corpo (404, 405, 415 etc.).
/// </summary>
public static class StatusCodeResponseExtensions
{
    public static IApplicationBuilder UseApiStatusCodeResponses(this IApplicationBuilder app)
    {
        return app.UseStatusCodePages(async context =>
        {
            var httpContext = context.HttpContext;
            var response = httpContext.Response;

            if (response.HasStarted)
            {
                return;
            }

            var (code, message) = response.StatusCode switch
            {
                StatusCodes.Status404NotFound => ("NOT_FOUND", "Endpoint não encontrado."),
                StatusCodes.Status405MethodNotAllowed => ("METHOD_NOT_ALLOWED", "Método HTTP não permitido para este endpoint."),
                StatusCodes.Status415UnsupportedMediaType => ("UNSUPPORTED_MEDIA_TYPE", "Content-Type inválido. Use o formato esperado pelo endpoint, normalmente application/json."),
                StatusCodes.Status401Unauthorized => ("UNAUTHORIZED", "A requisição requer autenticação válida."),
                StatusCodes.Status403Forbidden => ("FORBIDDEN", "Você não possui permissão para acessar este recurso."),
                _ => ("HTTP_ERROR", "A requisição não pôde ser processada.")
            };

            var payload = ApiResponse.ErrorResponse(message, code, new
            {
                traceId = httpContext.TraceIdentifier,
                method = httpContext.Request.Method,
                path = httpContext.Request.Path.Value,
                status = response.StatusCode
            });

            response.ContentType = "application/json; charset=utf-8";
            var body = JsonSerializer.Serialize(payload);
            await response.WriteAsync(body);
        });
    }
}
