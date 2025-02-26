using iChat.Data.Entities.Users;
using iChat.DTOs.Users.Auth;
using Microsoft.AspNetCore.Identity;

namespace iChat.BackEnd.Services.Users.Auth
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly IJwtService _jwtService;
        public AuthService(UserManager<AppUser> userManager, IJwtService jwtService)
        {
            _userManager = userManager;
            _jwtService = jwtService;
        }

        public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
        {
            var user = new AppUser
            {
                UserName = request.Email,
                Email = request.Email,
                FirstName = request.FirstName,
                LastName = request.LastName
            };
            var result = await _userManager.CreateAsync(user, request.Password);
            if (!result.Succeeded)
                throw new Exception(string.Join(", ", result.Errors.Select(e => e.Description)));
            return await GenerateAuthResponse(user);
        }
        public async Task<AuthResponse> LoginAsync(LoginRequest request)
        {
            var user = await _userManager.FindByNameAsync(request.Username);
            if (user == null || !await _userManager.CheckPasswordAsync(user, request.Password))
                throw new Exception("Invalid credentials");

            return await GenerateAuthResponse(user);
        }

        private async Task<AuthResponse> GenerateAuthResponse(AppUser user)
        {
            var roles = await _userManager.GetRolesAsync(user);
            var accessToken = await _jwtService.GenerateAccessToken(user);
            var refreshToken = _jwtService.GenerateRefreshToken();

            return new AuthResponse
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                Roles = roles.ToArray()
            };
        }
    }
}
