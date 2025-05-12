using iChat.BackEnd.Models.Helpers;
using iChat.BackEnd.Services.Users.Auth;
using iChat.DTOs.Users.Auth;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;

public class JwtService 
{
    private readonly JwtOptions _jwtOptions;
    private readonly RsaSecurityKey _rsaKey;
    private readonly JsonWebKey publicJwk;
    private readonly SigningCredentials _signingCredentials;
    private readonly DomainOptions _domainOptions;
    public JwtService(IOptions<JwtOptions> jwtOptions, IOptions<DomainOptions> domainOptions)
    {
        _jwtOptions = jwtOptions.Value;
        _domainOptions = domainOptions.Value;


        var rsa = RSA.Create(2048); 
        _rsaKey = new RsaSecurityKey(rsa)
        {
            KeyId = Guid.NewGuid().ToString() 
        };
        _signingCredentials = new SigningCredentials(_rsaKey, SecurityAlgorithms.RsaSha256);
        publicJwk= JsonWebKeyConverter.ConvertFromRSASecurityKey(
            new RsaSecurityKey(rsa.ExportParameters(false)) { KeyId = _rsaKey.KeyId });
    }
    
    public TokenResponse GenerateAccessToken(string userId, IList<string> roles=null)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, userId),
            new Claim(ClaimTypes.Name, Guid.NewGuid().ToString())
        };
        if(roles!=null)
        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));
        var expires= DateTime.UtcNow.AddMinutes(_jwtOptions.ExpireMinutes);
        var token = new JwtSecurityToken(
            issuer: _jwtOptions.Issuer,
            audience: _jwtOptions.Audience,
            claims: claims,
            expires: expires,
            signingCredentials: _signingCredentials
        );

        return new TokenResponse( new JwtSecurityTokenHandler().WriteToken(token),expires);
    }
    //public TokenResponse GrantToken(string userId, HttpContext httpContext)
    //{
    //    var token = GenerateAccessToken(userId);
    //    var cookieOptions = new CookieOptions
    //    {
    //        HttpOnly = false,
    //        Secure = true,
    //        SameSite = SameSiteMode.None,
    //        //Domain = _domainOptions.CookieDomain
    //    };
    //    httpContext.Response.Cookies.Append("access_token", token, cookieOptions);
    //}
    public TokenValidationParameters GetParam(bool validateLifetime)
    {
        return new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = _rsaKey,
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = validateLifetime,
            ClockSkew = TimeSpan.Zero
        };
    }

    public JsonWebKey GetPublicJwk()
    {
        Console.WriteLine($"JWKS modulus: {publicJwk.N}");
        return publicJwk;
    }
}
