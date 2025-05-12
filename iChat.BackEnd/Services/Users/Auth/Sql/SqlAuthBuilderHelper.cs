using iChat.BackEnd.Models.Helpers;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace iChat.BackEnd.Services.Users.Auth.Sql
{
    public class SqlAuthBuilderHelper
    {
        public void AddService(WebApplicationBuilder builder)
        {
            
            builder.Services.Configure<DomainOptions>(builder.Configuration.GetSection("DomainOptions"));
            builder.Services.Configure<RefreshTokenOptions>(builder.Configuration.GetSection("RefreshToken"));
            var jwtOptCol = builder.Configuration.GetSection("Jwt"); var jwtOptions = jwtOptCol.Get<JwtOptions>();
            builder.Services.Configure<JwtOptions>(jwtOptCol);
            builder.Services.AddSingleton<JwtService>();
            builder.Services.AddSingleton<RefreshTokenService>();
            builder.Services.AddScoped<SqlKeyRotationService>();
            builder.Services.AddTransient<ILoginService,SqlLoginService>();
            builder.Services.AddTransient<IRegisterService, CreateUserService>();
            builder.Services.AddSingleton<IPostConfigureOptions<JwtBearerOptions>, JwtBearerPostConfigureOptions>();
            builder.Services.AddAuthentication(options =>
            {
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                var jwtService = builder.Services.BuildServiceProvider().GetRequiredService<JwtService>();
                var publicKey = jwtService.GetPublicJwk();

                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = publicKey,
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };

                options.Events = new JwtBearerEvents
                {
                    OnAuthenticationFailed = context =>
                    {
                        // Logging logic
                        return Task.CompletedTask;
                    },
                    OnTokenValidated = context =>
                    {
                        // Logging logic
                        return Task.CompletedTask;
                    }
                };
            });

        }
    }
}
