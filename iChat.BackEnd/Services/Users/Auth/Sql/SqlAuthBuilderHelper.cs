using iChat.BackEnd.Models.Helpers;
using Microsoft.AspNetCore.Authentication.JwtBearer;
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
            builder.Services.AddTransient<ILoginService,SqlLoginService>();
            builder.Services.AddTransient<IRegisterService, CreateUserService>();
            builder.Services.PostConfigure<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme, options =>
            {
                var jwksService = builder.Services.BuildServiceProvider().GetRequiredService<JwtService>();
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = jwksService.GetPublicJwk(),
                    ValidateIssuer = true,
                    ValidIssuer = jwtOptions.Issuer,
                    ValidateAudience = true,
                    ValidAudience = jwtOptions.Audience,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };
            });


        }
    }
}
