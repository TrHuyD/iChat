﻿using iChat.Data.Entities.Users;
using iChat.Data.Entities.Users.Auth;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Threading.Tasks;

namespace iChat.BackEnd.Services.Users.Auth
{
    public interface IAccessTokenService
    {
        //TokenValidationParameters GetParam(bool vallifetime);
        //string GenerateAccessToken(Guid userId, IList<string> roles);
        //string GenerateRefreshToken();
        //ClaimsPrincipal? ValidateToken(string token, bool vallifetime = false);
        //bool isValid(string token);
        //bool ValidateRefreshToken(RefreshToken? token);
    }
}
