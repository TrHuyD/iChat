using iChat.BackEnd.Models.Infrastructures;
using iChat.BackEnd.Services.Users.Infra.IdGenerator;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;

namespace iChat.BackEnd.Services.Users.Auth.Auth0
{
    internal class Auth0BuilderHelper
    {
        public void AddService(WebApplicationBuilder builder)
        {

            Auth0Options auth0LoginOptions = builder.Configuration.GetSection("Auth0:Login").Get<Auth0Options>()?? throw new Exception("Auth0 Login key is missing");
            Auth0Options auth0RegisterOptions = builder.Configuration.GetSection("Auth0:Register").Get<Auth0Options>()?? throw new Exception("Auth0 Register key is missing") ;

            var domain = $"https://{builder.Configuration["Auth0:Domain"]}/";
            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.Authority = domain;
                options.Audience = builder.Configuration["Auth0:Audience"];
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    NameClaimType = ClaimTypes.NameIdentifier
                };
            });
            builder.Services.AddSingleton<IRegisterService>(provider =>
            {
                return new Auth0RegisterService(auth0RegisterOptions, provider.GetService<UserIdService>());
            });
            builder.Services.AddTransient<ILoginService>(provider=>
            {
                return new Auth0LoginService(auth0LoginOptions, provider.GetService<IHttpContextAccessor>());
            });
        }
    }
}
