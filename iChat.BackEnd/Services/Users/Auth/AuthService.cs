using iChat.Data.Entities.Users;
using iChat.Data.Entities.Users.Auth;
using iChat.DTOs.Users.Auth;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

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
            var accessToken = _jwtService.GenerateAccessToken(user.Id,await _userManager.GetRolesAsync(user));
            var refreshToken = _jwtService.GenerateRefreshToken();
            user.RefreshTokens.Add(new RefreshToken
            {
                Token = refreshToken,
                ExpiryDate = DateTime.UtcNow.AddDays(7),
                Created = DateTime.UtcNow,
                UserId = user.Id
            });
            await _userManager.UpdateAsync(user);
            return new AuthResponse
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                Roles = roles.ToArray()
            };
        }

        public async Task<RefreshTokenRespone> RefreshTokenAsync(RefreshTokenRequest request)
        {
            var t = _jwtService.ValidateJwtToken(request.Jwt);
            var userIdClaim = t?.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdClaim) )
                throw new Exception("Invalid token");

            var user = await _userManager.Users.Include(u => u.RefreshTokens).FirstOrDefaultAsync(u => u.Id == Guid.Parse(userIdClaim));
            if (user == null)
                throw new Exception("User not found");

            var refreshToken = user.RefreshTokens?.FirstOrDefault(t => t.Token == request.RefreshToken);
            if (refreshToken == null || !_jwtService.ValidateRefreshToken(refreshToken))
                throw new Exception("Invalid refresh token");

            var roles = await _userManager.GetRolesAsync(user);
            var accessToken = _jwtService.GenerateAccessToken(user.Id, roles);

            return new RefreshTokenRespone { Token = accessToken };
        }

        public Task<bool> isValidJwt(RefreshTokenRequest request)
        {
            return Task.FromResult(_jwtService.isValid(request.Jwt));
        }
    }
}
