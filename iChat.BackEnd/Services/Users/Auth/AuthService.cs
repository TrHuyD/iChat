using iChat.Data.Entities.Users;
using iChat.DTOs.Users.Auth;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace iChat.BackEnd.Services.Users.Auth
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly UserRelationService _neo4jService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuthService(UserRelationService neo4JService,UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, IHttpContextAccessor httpContextAccessor)
        {
            _neo4jService = neo4JService;
            _userManager = userManager;

            _signInManager = signInManager;
            _httpContextAccessor = httpContextAccessor;
        }



        public async Task<bool> LoginAsync(LoginRequest request)
        {
            var user = await _userManager.FindByNameAsync(request.Username);
            if (user == null || !await _userManager.CheckPasswordAsync(user, request.Password))
                throw new Exception("Invalid credentials");
            await _signInManager.SignInAsync(user, isPersistent: request.RememberMe);

            return true;
        }

        public async Task LogoutAsync()
        {
            await _signInManager.SignOutAsync();
        }
    }
}
