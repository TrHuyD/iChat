using iChat.Data.Entities.Users;
using iChat.DTOs.Users.Auth;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

using iChat.DTOs.Shared;
using iChat.BackEnd.Models.User;
using Microsoft.Extensions.Options;
using iChat.BackEnd.Models.Helpers;

using iChat.Data.EF;
using Microsoft.EntityFrameworkCore;

namespace iChat.BackEnd.Services.Users.Auth.Sql
{
    public class SqlLoginService : ILoginService
    {
        private readonly UserManager<AppUser> _userManager;
        //private readonly SignInManager<AppUser> _signInManager;

        private readonly RefreshTokenService _rfService;
        private readonly DomainOptions _domainOptions;
        private readonly iChatDbContext _dbContext;
        public SqlLoginService(UserManager<AppUser> userManager
            /*SignInManager<AppUser> signInManager*/,
            RefreshTokenService rfService,
            iChatDbContext dbContext
            ,
            IOptions<DomainOptions> domainOptions

            )
        {
            _domainOptions = domainOptions.Value;

            _userManager = userManager;
            _rfService = rfService;
            _dbContext = dbContext;
            //_signInManager = signInManager;
        }



        public async Task<OperationResult> LoginAsync(LoginRequest request,HttpContext context)
        {
            var user = await _userManager.FindByNameAsync(request.Username);
            if (user == null || !await _userManager.CheckPasswordAsync(user, request.Password))
                return OperationResult.Fail("401","Invalid credential");
            _rfService.NewLogin(user, context);

            await _dbContext.SaveChangesAsync();
            return OperationResult.Ok();
        }

        public async Task LogoutAsync(HttpContext _context)
        {
            _context.Response.Cookies.Delete("refreshToken", new CookieOptions
            {
                Path = "/refreshtoken",
                Domain = _domainOptions.CookieDomain
            });
            _ = Task.Run(async () =>
            {
                var token = _context.Request.Cookies["refreshToken"];
                if (token != null)
                {
                    var rf = await _dbContext.RefreshTokens.FirstOrDefaultAsync(x => x.Token == token);
                    if (rf != null)
                    {
                        _rfService.Revoke(rf);
                        await _dbContext.SaveChangesAsync();
                    }
                }
            });
            //await _signInManager.SignOutAsync();
        }
    }
}
