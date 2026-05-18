namespace BibliotecaApi.Application.Api.Auth;

public sealed class ApiTokenSettings
{
    public const string SectionName = "ApiToken";

    public string Token { get; set; } = string.Empty;
}
