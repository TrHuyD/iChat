using Auth0.AuthenticationApi.Models;
using Auth0.AuthenticationApi;
using Auth0.ManagementApi.Models;
using Auth0.ManagementApi;
using iChat.BackEnd.Models.Infrastructures;
using iChat.DTOs.Users.Auth;
using iChat.BackEnd.Services.Users.Infra.IdGenerator;
using iChat.DTOs.Shared;
using Auth0.AspNetCore.Authentication.Exceptions;
using Microsoft.Extensions.Options;

namespace iChat.BackEnd.Services.Users.Auth.Auth0
{
    public class Auth0RegisterService :IRegisterService
    {
        private readonly Auth0Options _settings;
        private readonly UserIdService _idService;

        public Auth0RegisterService(Auth0Options settings, UserIdService idService)
        {
            _settings = settings;
            _idService = idService;
        }

     

        public async Task<OperationResult> RegisterAsync(RegisterRequest rq)
        {
            try
            {
                var managementToken = await GetManagementApiTokenAsync();

                var managementClient = new ManagementApiClient(managementToken, new Uri(_settings.Audience));

                var userCreateRequest = new UserCreateRequest
                {
                    Connection = _settings.Connection,
                    UserName = rq.UserName,
                    Email = rq.Email,
                    UserId = _idService.GenerateId().ToString(),
                    Password = rq.Password,
                    EmailVerified = false
                };

                var user = await managementClient.Users.CreateAsync(userCreateRequest);
                return OperationResult.Ok();
            }
            catch (ErrorApiException ex)
            {
                Console.Error.WriteLine("Auth0 user creation failed: " + ex.ApiError.Message);
                return OperationResult.Fail(ex.StatusCode.ToString(), ex.ApiError.Message);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("Unexpected error: " + ex.Message);
                return OperationResult.Fail("server_error", "An unexpected error occurred.");
            }
        }
        private string _cachedManagementToken="";
        private DateTime _managementTokenExpiry=DateTime.MinValue;
        private async Task<string> GetManagementApiTokenAsync()
        {
            if (_managementTokenExpiry > DateTime.UtcNow)
            {
                return _cachedManagementToken;
            }

            var authClient = new AuthenticationApiClient(new Uri($"https://{_settings.Domain}"));

            var tokenRequest = new ClientCredentialsTokenRequest
            {
                ClientId = _settings.ClientId,
                ClientSecret = _settings.ClientSecret,
                Audience = _settings.Audience
            };

            var tokenResponse = await authClient.GetTokenAsync(tokenRequest);

            _cachedManagementToken = tokenResponse.AccessToken;
            _managementTokenExpiry = DateTime.UtcNow.AddSeconds(tokenResponse.ExpiresIn - 60);

            return _cachedManagementToken;
        }
    }
}