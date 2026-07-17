using System.Diagnostics.CodeAnalysis;
using System.Text;
using BeTiny.Application.Common.Options;
using BeTiny.Domain.Common.Interfaces;
using BeTiny.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace BeTiny.IOC.DependencyInjections;

[ExcludeFromCodeCoverage]
public static class ConfigureAuthentication
{
    public static IServiceCollection RegisterAuthentication(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUser, CurrentUser>();

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(
                options =>
                {
                    var jwt = configuration.GetSection("Jwt")
                        .Get<JwtOptions>()!;

                    options.MapInboundClaims = false;
                    options.TokenValidationParameters = new ()
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,

                        ValidIssuer = jwt.Issuer,
                        ValidAudience = jwt.Audience,
                        IssuerSigningKey = new SymmetricSecurityKey(
                            Encoding.UTF8.GetBytes(jwt.SigningKey ?? string.Empty)
                        ),

                        ClockSkew = TimeSpan.Zero
                    };
                }
            );

        services.AddAuthorization();

        return services;
    }
}
