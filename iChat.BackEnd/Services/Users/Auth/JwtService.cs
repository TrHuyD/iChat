using iChat.BackEnd.Models.Helpers;
using iChat.BackEnd.Services.Users.Auth;
using iChat.Data.Entities.Users.Auth;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

public class JwtService : IJwtService
{
    private JwtOptions _jwtOptions => __jwtOptions.CurrentValue;
    private readonly IOptionsMonitor<JwtOptions> __jwtOptions;

    public JwtService(IOptionsMonitor<JwtOptions> jwtOptions)
    {
        __jwtOptions = jwtOptions;
    }
    public TokenValidationParameters GetParam(bool vallifetime)
    {
        var key = Encoding.UTF8.GetBytes(_jwtOptions.SecretKey);
        return  new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = vallifetime,
            ClockSkew = TimeSpan.Zero
        };
    }
    public string GenerateAccessToken(Guid userId, IList<string> roles)
    {
        var claims = new List<Claim>
    {
        new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
        new Claim(ClaimTypes.Name, Guid.NewGuid().ToString())
    };

        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.SecretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _jwtOptions.Issuer,
            audience: _jwtOptions.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_jwtOptions.ExpireMinutes),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public string GenerateRefreshToken()
    {
        var randomNumber = new byte[32];
        RandomNumberGenerator.Fill(randomNumber);
        return WebEncoders.Base64UrlEncode(randomNumber);
    }
    public bool ValidateRefreshToken(RefreshToken? token)
    {
        if(token == null||token.IsExpired||token.Revoked is not null)
            return false;
        return true;
    }
    public ClaimsPrincipal? ValidateJwtToken(string token,bool vallifetime=false)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(_jwtOptions.SecretKey);

        var validateParam = GetParam(vallifetime);
        try
        {
            var principal = tokenHandler.ValidateToken(token, validateParam, out var validatedToken);
            if (validatedToken is JwtSecurityToken jwtToken)
            {
                var correctAlg = jwtToken.SignatureAlgorithm == SecurityAlgorithms.HmacSha256;
                if (!correctAlg) return null;
            }
            else
            {
                return null;
            }

            return principal;
        }
        catch
        {
            return null;
        }
    }

    public bool isValid(string token)
    {
        return ValidateJwtToken(token,true) != null;
    }
}
