using iChat.BackEnd.Models.Helpers;
using iChat.BackEnd.Services.Users.ChatServers;
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
            builder.Logging.ClearProviders();
            builder.Logging.AddConsole();
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

            builder.Services.AddSignalR()
            .AddJsonProtocol()
            .AddHubOptions<ChatHub>(options =>
            {
                options.EnableDetailedErrors = true;
                options.ClientTimeoutInterval = TimeSpan.FromMinutes(1);
            });
            builder.Services.AddAuthentication(options =>
            {
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
             .AddJwtBearer();


        }
    }
}
