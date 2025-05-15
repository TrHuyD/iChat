

using iChat.BackEnd.Models.Helpers;
using iChat.BackEnd.Models.User;
using iChat.Data.Entities.Users;
using iChat.Data.Entities.Users.Auth;
using iChat.DTOs.Shared;
using iChat.DTOs.Users;
using iChat.DTOs.Users.Auth;
using Microsoft.Extensions.Options;
using System.Net;
using System.Security.Cryptography;

namespace iChat.BackEnd.Services.Users.Auth
{
    public class RefreshTokenService
    {

        public RefreshTokenOptions options { get; init; }
        DomainOptions DomainOptions;
        public RefreshTokenService(IOptions<RefreshTokenOptions> options,IOptions<DomainOptions> domainOptions)
        {
            this.options = options.Value;
            DomainOptions = domainOptions.Value;
        }
//To do : improve this but too lazy
        private static string GenerateToken()
        {
            var randomNumber = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomNumber);
                var refreshToken = Convert.ToBase64String(randomNumber);


                return refreshToken;
            }
        }
        public bool isUsable(RefreshToken? refreshToken)
        {
            if(refreshToken == null || refreshToken.ExpiryDate < DateTime.UtcNow || refreshToken.Revoked is not null)
                return false;
            return true;
        }
        public void RefreshIfNeeded(RefreshToken rf, HttpContext context)
        {
            if(rf.Created.AddDays(options.LiveDays-options.RefreshDays) < DateTime.Now)
            {
                RenewRefreshToken(rf);
            }
        }
        private void RenewRefreshToken(RefreshToken rf)
        {
            rf.Created = DateTime.Now;
            rf.Revoked = null;
        }
        public void Refresh(RefreshToken rf,HttpContext context)
        {
            var token = GenerateToken();
            rf.Token = token;
            RenewRefreshToken(rf);
            AssignRFTokenCookie(context, rf);
            //return rf;
        }
        public void Revoke(RefreshToken rf)
        {
            rf.Revoked = DateTime.MinValue;
        }
        private RefreshTokenDto toDto(RefreshToken rf)
        {
            var token = GenerateToken();
            rf.Token = token;
            //RenewRefreshToken(rf);
            return new RefreshTokenDto()
            {
                Token = token,
                ExpiryDate = rf.ExpiryDate
            };
        }
        private void AssignRFTokenCookie(HttpContext context, RefreshToken rf)
        {
            var token = GenerateToken();
            rf.Token = token;
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Expires = rf.ExpiryDate,
                Secure = true,
                SameSite = SameSiteMode.None,
                Path = UrlPath.RefreshTokenApi,
                Domain = DomainOptions.CookieDomain,
            };
            context.Response.Cookies.Append("refreshToken", token, cookieOptions);
        }
        public void NewLogin(AppUser user,HttpContext context)
        {
            var token = GenerateToken();
            var Created = DateTime.Now;
            var rf = new RefreshToken()
            {
                Created = Created,
                ExpiryDate = Created.AddDays(options.LiveDays),
                Token = token,
                UserId = user.Id,
                IpAddress =  context.Connection.RemoteIpAddress.ToString(),

            };
            user.RefreshTokens.Add(rf);
            AssignRFTokenCookie(context, rf);

        }
    }
}
