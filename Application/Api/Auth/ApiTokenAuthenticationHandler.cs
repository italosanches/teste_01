using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using BibliotecaApi.Application.Api.Responses;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace BibliotecaApi.Application.Api.Auth;

public sealed class ApiTokenAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    private readonly ApiTokenSettings _settings;

    public ApiTokenAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        IOptions<ApiTokenSettings> settings)
        : base(options, logger, encoder)
    {
        _settings = settings.Value;
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (string.IsNullOrWhiteSpace(_settings.Token))
            return Task.FromResult(AuthenticateResult.Fail("Token da API não configurado no servidor."));

        if (!TryExtractToken(Request, out var tokenRecebido))
            return Task.FromResult(AuthenticateResult.Fail("Token não informado."));

        if (!TokensIguais(tokenRecebido, _settings.Token))
            return Task.FromResult(AuthenticateResult.Fail("Token inválido."));

        var principal = new System.Security.Claims.ClaimsPrincipal(
            new System.Security.Claims.ClaimsIdentity(
                new[] { new System.Security.Claims.Claim("auth", "api-token") },
                Scheme.Name));

        return Task.FromResult(AuthenticateResult.Success(new AuthenticationTicket(principal, Scheme.Name)));
    }

    protected override async Task HandleChallengeAsync(AuthenticationProperties properties)
    {
        Response.StatusCode = StatusCodes.Status401Unauthorized;
        Response.ContentType = "application/json";

        var mensagem = properties.GetString("mensagem")
            ?? "Token inválido ou não informado. Use o header Authorization: Bearer {seu_token}.";

        await Response.WriteAsJsonAsync(ApiResponse<object>.Falha(mensagem));
    }

    private static bool TryExtractToken(HttpRequest request, out string token)
    {
        token = string.Empty;

        var authorization = request.Headers.Authorization.ToString();
        if (!string.IsNullOrWhiteSpace(authorization)
            && authorization.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            token = authorization["Bearer ".Length..].Trim();
            return !string.IsNullOrWhiteSpace(token);
        }

        if (request.Headers.TryGetValue("X-Api-Token", out var apiTokenHeader)
            && !string.IsNullOrWhiteSpace(apiTokenHeader))
        {
            token = apiTokenHeader.ToString().Trim();
            return true;
        }

        return false;
    }

    private static bool TokensIguais(string recebido, string esperado)
    {
        var bytesRecebido = Encoding.UTF8.GetBytes(recebido);
        var bytesEsperado = Encoding.UTF8.GetBytes(esperado);

        return bytesRecebido.Length == bytesEsperado.Length
            && CryptographicOperations.FixedTimeEquals(bytesRecebido, bytesEsperado);
    }
}
