using Microsoft.Extensions.Caching.Memory;
using VectorStinger.Core.Domain.Managers.Security.AccountManager.ValidateToken;
using VectorStinger.Core.Interfaces.Managers.Security;

namespace VectorStinger.Api.Service.Middleware;

public class SessionTokenValidationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IMemoryCache _cache;

    // Paths que se deben excluir de la validación de token
    private static readonly string[] ExcludedPaths = new[]
    {
        "/VerifyCredentialOAuthUseCase", // Path exacto del login OAuth
        "/",                             // Path raíz (GET "/")
        "/swagger",                      // Swagger UI
        "/swagger/index.html",           // Swagger UI principal
        "/swagger/v1/swagger.json",      // Swagger JSON
        "/ValidateTokenUseCase",
        "/health",                       // Health check endpoint
        "/alive",                        // Alive endpoint
        "/ready",                        // Ready endpoint
    };

    public SessionTokenValidationMiddleware(RequestDelegate next, IMemoryCache cache)
    {
        _next = next;
        _cache = cache;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var requestPath = context.Request.Path.Value ?? string.Empty;
        var isSwagger = requestPath.StartsWith("/swagger", StringComparison.OrdinalIgnoreCase);

        // Excluir paths definidos y cualquier recurso bajo /swagger
        if (ExcludedPaths.Any(p => string.Equals(requestPath, p, StringComparison.OrdinalIgnoreCase)) || isSwagger)
        {
            await _next(context);
            return;
        }

        var token = context.Request.Headers["Authorization"].FirstOrDefault()?.Replace("Bearer ", "");
        if (string.IsNullOrEmpty(token))
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsync("Token de sesión requerido.");
            return;
        }


        ValidateTokenResponse? validateTokenResponse = null;
        if (!_cache.TryGetValue(token, out validateTokenResponse))
        {
            // Crear un scope para obtener el servicio scoped
            using var scope = context.RequestServices.CreateScope();
            var accountManager = scope.ServiceProvider.GetRequiredService<IAccountManager>();

            var resultTokenValidation = accountManager.ValidateTokenAsync(new ValidateTokenRequest
            {
                Token = token
            });

            if (resultTokenValidation.IsSuccess && resultTokenValidation.Value != null && resultTokenValidation.Value.IsValid)
            {
                var tiempoRestante = resultTokenValidation.Value.TimeExpired - DateTime.Now;
                if (tiempoRestante > TimeSpan.Zero)
                {
                    _cache.Set(token, resultTokenValidation.Value, tiempoRestante);
                }
                validateTokenResponse = resultTokenValidation.Value;
            }
            else
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                var errorMsg = resultTokenValidation.Errors?.FirstOrDefault()?.Message ?? "Token de sesión inválido o expirado.";
                await context.Response.WriteAsync(errorMsg);
                return;
            }
        }

        if (validateTokenResponse == null || !validateTokenResponse.IsValid || validateTokenResponse.TimeExpired <= DateTime.Now)
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsync("Token de sesión inválido o expirado.");
            return;
        }

        await _next(context);
    }
}