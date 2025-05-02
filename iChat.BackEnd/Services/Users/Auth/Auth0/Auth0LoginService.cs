using Auth0.AuthenticationApi.Models;
using Auth0.AuthenticationApi;
using iChat.BackEnd.Models.Infrastructures;
using iChat.BackEnd.Services.Users.Infra.IdGenerator;
using Microsoft.AspNetCore.Http;
using System.Runtime;
using iChat.DTOs.Users.Auth;
using iChat.DTOs.Shared;
using Auth0.AspNetCore.Authentication.Exceptions;
using Microsoft.Extensions.Options;

namespace iChat.BackEnd.Services.Users.Auth.Auth0
{
    public class Auth0LoginService :ILoginService
    {
        Auth0Options _settings;
        private readonly HttpContext httpContext;
        public Auth0LoginService(Auth0Options settings, IHttpContextAccessor contextAccessor)
        {
            _settings = settings;
            httpContext = contextAccessor.HttpContext ?? throw new ArgumentNullException(nameof(contextAccessor.HttpContext));

        }
        public async Task<OperationResult> LoginAsync(LoginRequest rq)
        {
            try
            {
                var authClient = new AuthenticationApiClient(new Uri($"https://{_settings.Domain}"));

                var tokenRequest = new ResourceOwnerTokenRequest
                {
                    ClientId = _settings.ClientId,
                    ClientSecret = _settings.ClientSecret,
                    Scope = "openid profile email offline_access",
                    Realm = _settings.Connection,
                    Username = rq.Username,
                    Password = rq.Password,
                    Audience = _settings.Audience
                };

                var tokenResponse = await authClient.GetTokenAsync(tokenRequest);

                // Set access/refresh token in HttpOnly cookie
                httpContext.Response.Cookies.Append("access_token", tokenResponse.AccessToken, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Strict,
                    Expires = DateTime.UtcNow.AddSeconds(tokenResponse.ExpiresIn)
                });


                if (!string.IsNullOrEmpty(tokenResponse.RefreshToken))
                {
                    httpContext.Response.Cookies.Append("refresh_token", tokenResponse.RefreshToken, new CookieOptions
                    {
                        HttpOnly = true,
                        Secure = true,
                        SameSite = SameSiteMode.Strict,
                        Expires = DateTime.UtcNow.AddDays(14)
                    });
                }
                return OperationResult.Ok();
            }
            catch (ErrorApiException ex)
            {
                Console.Error.WriteLine("Auth0 user login failed: " + ex.ApiError.Message);
                return OperationResult.Fail(ex.StatusCode.ToString(), ex.ApiError.Message);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("Unexpected error: " + ex.Message);
                return OperationResult.Fail("server_error", "An unexpected error occurred.");
            }
        }
    }
}
