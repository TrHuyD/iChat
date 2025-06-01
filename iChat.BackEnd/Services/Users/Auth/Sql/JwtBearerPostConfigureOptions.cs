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
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Cookies["access_token"];
                var path = context.HttpContext.Request.Path;
                _logger.LogInformation($"Received SignalR access_token: {accessToken}/{path}" );
                if (!string.IsNullOrEmpty(accessToken) &&
                    path.StartsWithSegments("/api/chathub"))
                {
                  
                    context.Token = accessToken;
                }

                return Task.CompletedTask;
            },
            OnAuthenticationFailed = context =>
            {
                _logger.LogWarning(context.Exception,
                    "JWT Authentication Failed: {Message}",
                    context.Exception?.Message);
                return Task.CompletedTask;
            },
            OnTokenValidated = context =>
            {
                _logger.LogInformation("JWT token validated successfully.");
                return Task.CompletedTask;
            }
        };
    }
}
