using iChat.BackEnd.Models.Helpers;
using iChat.BackEnd.Models.User;
using iChat.Data.EF;
using iChat.DTOs.Shared;
using iChat.DTOs.Users.Auth;
using Microsoft.EntityFrameworkCore;

namespace iChat.BackEnd.Services.Users.Auth.Sql
{
    public class SqlKeyRotationService
    {
        private readonly JwtService _accessKeyService;
        private readonly RefreshTokenService _refreshTokenService;
        private readonly iChatDbContext _dbContext;

        public SqlKeyRotationService(
            JwtService jwksService,
            RefreshTokenService refreshTokenService,
            iChatDbContext context)
        {
            _accessKeyService = jwksService;
            _refreshTokenService = refreshTokenService;
            this._dbContext = context;

        }
        public async Task<OperationResultT<TokenResponse>> RefreshCred(HttpContext _context)
        {
            var token = _context.Request.Cookies["refreshToken"];
            if (string.IsNullOrEmpty(token))
                return OperationResultT<TokenResponse>.Fail("401", "Invalid credential, refreshtoken is missing");
            var refreshToken = await _dbContext.RefreshTokens
                .Include(r => r.User)
                .FirstOrDefaultAsync(r => r.Token == token && r.Revoked == null);
            if (!_refreshTokenService.isUsable(refreshToken))
                return OperationResultT<TokenResponse>.Fail("401", "Invalid credential, unusable token");
            _refreshTokenService.RefreshIfNeeded(refreshToken, _context);

            var accesstoken = _accessKeyService.GenerateAccessToken(refreshToken.User.Id.ToString());
            _accessKeyService.AssignToken(accesstoken, _context);


            return OperationResultT<TokenResponse>.Ok(accesstoken);

        }
    }
}
