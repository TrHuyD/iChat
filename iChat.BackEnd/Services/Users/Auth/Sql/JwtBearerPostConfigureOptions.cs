using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

public class JwtBearerPostConfigureOptions : IPostConfigureOptions<JwtBearerOptions>
{
    private readonly JwtService _jwtService;
    private readonly ILogger<JwtBearerPostConfigureOptions> _logger;

    public JwtBearerPostConfigureOptions(
        JwtService jwtService,
        ILogger<JwtBearerPostConfigureOptions> logger)
    {
        _jwtService = jwtService;
        _logger = logger;
    }

    public void PostConfigure(string name, JwtBearerOptions options)
    {
        var publicKey = _jwtService.GetPublicJwk();

        // Optional: Add null check for publicKey
        if (publicKey == null)
        {
            _logger.LogError("Public JWT key is null. Authentication configuration may fail.");
        }

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = publicKey,
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };

        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
               
                _logger.LogError(context.Exception,
                    "JWT Authentication Failed: {ExceptionMessage}",
                    context.Exception?.Message);
                return Task.CompletedTask;
            },
            OnTokenValidated = context =>
            {
                // Log token validation success
                _logger.LogInformation("JWT token validated successfully");
                return Task.CompletedTask;
            }
        };
    }
}

