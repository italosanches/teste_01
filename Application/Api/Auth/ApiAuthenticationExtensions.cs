using Microsoft.AspNetCore.Authentication;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace BibliotecaApi.Application.Api.Auth;

public static class ApiAuthenticationExtensions
{
    public static IServiceCollection AddApiTokenAuthentication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<ApiTokenSettings>(configuration.GetSection(ApiTokenSettings.SectionName));

        services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = ApiTokenAuthenticationDefaults.Scheme;
                options.DefaultChallengeScheme = ApiTokenAuthenticationDefaults.Scheme;
            })
            .AddScheme<AuthenticationSchemeOptions, ApiTokenAuthenticationHandler>(
                ApiTokenAuthenticationDefaults.Scheme,
                _ => { });

        services.AddAuthorization();

        return services;
    }

    public static void ConfigureApiTokenSwagger(SwaggerGenOptions options)
    {
        options.AddSecurityDefinition("ApiToken", new OpenApiSecurityScheme
        {
            Description = "API Token. Informe: Bearer {seu_token}",
            Name = "Authorization",
            In = ParameterLocation.Header,
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "Token"
        });

        options.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "ApiToken"
                    }
                },
                Array.Empty<string>()
            }
        });
    }
}
