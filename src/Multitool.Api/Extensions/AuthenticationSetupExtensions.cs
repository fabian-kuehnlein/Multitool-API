using System.Text;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Multitool.Api.Authentication;
using Multitool.Domain.Exceptions;

namespace Multitool.Api.Extensions;

public static class AuthenticationSetupExtensions
{
    public static IServiceCollection AddApiAuthentication(
        this IServiceCollection services,
        IConfiguration configuration,
        IWebHostEnvironment environment)
    {
        // The hard-coded test identity must not replace real JWT authentication
        // in local development; otherwise every request runs as the same user
        // and per-user data isolation cannot be verified.
        if (environment.IsEnvironment("Testing"))
        {
            services.AddAuthentication(TestAuthHandler.SchemeName)
                .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(TestAuthHandler.SchemeName, _ => { });

            return services;
        }

        var jwtSettings = configuration.GetSection("Jwt");
        var jwtKey = configuration["Jwt:Key"]
            ?? throw new JwtMissingException("JWT key is missing.");

        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new()
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtSettings["Issuer"],
                    ValidAudience = jwtSettings["Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
                };
            });

        return services;
    }
}
