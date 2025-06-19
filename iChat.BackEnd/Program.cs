using iChat.BackEnd.Models.Helpers;
using iChat.BackEnd.Services.Users;
using iChat.BackEnd.Services.Users.Auth;
using iChat.Data.EF;
using iChat.Data.Entities.Users;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Neo4j.Driver;
using System;
using iChat.Data.Entities.Users.Messages;
using iChat.BackEnd.Services.Users.Infra.CassandraDB;
using Microsoft.Extensions.DependencyInjection;

using iChat.BackEnd.Services.Users.Infra.IdGenerator;
using iChat.BackEnd.Models.Infrastructures;
using Microsoft.OpenApi.Models;
using iChat.BackEnd.Services.Users.Infra.Neo4jService;
using iChat.BackEnd.Services.Users.Infra.Redis;
using iChat.BackEnd.Services.Users.Infra.Helpers;
using iChat.BackEnd.Models.Helpers.CassandraOptionss;
using iChat.BackEnd.Services.Validators;
using iChat.BackEnd.Services.Validators.TextMessageValidators;
using iChat.BackEnd.Services.Users.ChatServers;
using iChat.BackEnd.Services.Users.Infra.Redis.MessageServices;
using iChat.BackEnd.Services.Users.Infra.Redis.ChatServerServices;
using iChat.BackEnd.Services.StartUpServices.SUS_ChatServer;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using iChat.BackEnd.Services.Users.Auth.Sql;
using iChat.Client;
using System.Text.Json;
using iChat.BackEnd.Controllers.UserControllers.MessageControllers;
using iChat.BackEnd.Services.Users.ChatServers.Abstractions;
using iChat.BackEnd.Services.Users.Infra.EfCore.MessageServices;
//using Microsoft.AspNetCore.Authentication;
//using Microsoft.Extensions.DependencyInjection;
var builder = WebApplication.CreateBuilder(args);
if (builder.Environment.IsDevelopment())
{
    builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new OpenApiInfo { Title = "My API", Version = "v1" });
    });
    builder.Services.AddControllers();
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowClient", policy =>
        {
            policy.WithOrigins("https://localhost:7156")
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials();
        });
    });


}
else
{
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowClient",
           builder => builder.WithOrigins("https://trhuyd.github.io")
                             .AllowAnyMethod()
                             .AllowAnyHeader()
                             .AllowCredentials());
    });

}
// Add services to the container.
builder.Services.AddControllers();//.AddRazorRuntimeCompilation();

builder.Configuration.AddUserSecrets<Program>();
builder.Services.AddHttpContextAccessor();  
//var neo4jConfig = builder.Configuration.GetSection("Neo4j");

var WorkerIdConfig = new ConfigurationBuilder()
    .AddJsonFile("workerSettings.json", optional: true, reloadOnChange: true)
    .Build().GetSection("Id").Get<WorkerID>();
builder.Services.AddSingleton<ThreadSafeCacheService>();
builder.Services.AddSingleton<IRedisConnectionService>(provider =>
{
    var configuration = provider.GetRequiredService<IConfiguration>();
    return new CloudRedisCacheService(configuration);
});
builder.Services.AddSingleton<AppRedisService>();
builder.Services.AddSingleton<RedisLiveTime>();
builder.Services.AddSingleton<RedisChatServerService>();
new IdBuilderHelper().AddService(builder, WorkerIdConfig);
//new Auth0BuilderHelper().AddService(builder);
//builder.Services.AddSingleton<IDriver>(GraphDatabase.Driver(neo4jConfig["Uri"], AuthTokens.Basic(neo4jConfig["Username"], neo4jConfig["Password"]!)));
builder.Services.AddTransient<IAsyncSession>(provider =>
{
    var driver = provider.GetRequiredService<IDriver>();
    return driver.AsyncSession();
});
builder.Services.AddTransient(provider =>
    new Lazy<IAsyncSession>(() => provider.GetRequiredService<IAsyncSession>()));
new CassandraBuilderHelper().AddService(builder);
new ValidatorsHelper(builder);
//builder.Services.AddSingleton<UserSendTextMessageService>();
//builder.Services.AddTransient<Neo4jChatChannelEditService>();
builder.Services.AddTransient<IChatServerEditService,EfCoreChatServerEditService>();
//builder.Services.AddTransient(provider =>
//    new Lazy<Neo4jChatListingService>(() => provider.GetRequiredService<Neo4jChatListingService>()));
builder.Services.AddTransient<IChatListingService,EfCoreChatListingService>();

builder.Services.AddTransient<RedisUserServerService>();
builder.Services.AddTransient<RedisChatCache>();
builder.Services.AddTransient<RedisSegmentCache>();


builder.Services.AddTransient<ServerListService>();

builder.Services.AddSingleton<IChatSendMessageService, Test_UserSendTextMessageService > ();
builder.Services.AddSingleton<IChatReadMessageService, Test_UserChatReadMessageService>();


// Database Context
builder.Services.AddDbContext<iChatDbContext>(options =>
    options.UseSqlite($"Data Source={builder.Configuration.GetValue<string>("ConnectionStrings:sqlite")};"));

// Identity Configuration with Cookie Authentication
builder.Services.AddIdentity<AppUser, Role>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
})
    .AddEntityFrameworkStores<iChatDbContext>()
    .AddDefaultTokenProviders();
new SqlAuthBuilderHelper().AddService(builder);
// Configure Cookie Authentication
//builder.Services.ConfigureApplicationCookie(options =>
//{
//    options.LoginPath = "/Login";
//    options.LogoutPath = "/Logout";
//    options.AccessDeniedPath = "/Forbidden";
//    options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
//    options.SlidingExpiration = true;
//});

////JWT Authentication
////JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

//builder.Services
//    .AddAuth0WebAppAuthentication(options => {
//        options.Domain = builder.Configuration["Auth0:Domain"];
//        options.ClientId = builder.Configuration["Auth0:ClientId"];
//    });


builder.Services.AddTransient<IChatCreateService,EfCoreChatCreateService>();
//builder.Services.AddTransient < Neo4jCreateUserService>();
builder.Services.AddTransient<CreateUserService>();
builder.Services.AddTransient<IUserService, UserService>();
//builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IPublicUserService, PublicUserService>();

builder.Services.AddHostedService<SUS_ServerChannelCacheLoader>();
builder.Services.AddEndpointsApiExplorer();


builder.Services.AddHttpContextAccessor();

//// Configure Authentication
//builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
//    .AddCookie();
//builder.Services.ConfigureApplicationCookie(options =>
//{
//    options.Events.OnRedirectToLogin = context =>
//    {
//        if (context.Request.Path.StartsWithSegments("/api"))
//        {
//            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
//            context.Response.ContentType = "application/json";
//            return context.Response.WriteAsync(JsonSerializer.Serialize(new
//            {
//                error = "Unauthorized",
//                message = "Authentication required"
//            }));
//        }

//        context.Response.Redirect(context.RedirectUri);
//        return Task.CompletedTask;
//    };
//});

// Configure Logging

var app= builder.Build();

// --- Middleware pipeline ---

app.Use(async (context, next) =>
{
    var path = context.Request.Path;
    Console.WriteLine($"IN->Request: {context.Request.Method} {path}");

    await next();

    var type = context.Response.ContentType ?? "none";
    Console.WriteLine($"OUT<- Response: {context.Response.StatusCode} ({type})");

    if (path.StartsWithSegments("/api") &&
        context.Response.StatusCode == 200 &&
        type.Contains("text/html", StringComparison.OrdinalIgnoreCase))
    {
        Console.WriteLine("API returned HTML — probably hit the Blazor fallback.");
    }
});
app.Use(async (context, next) =>
{
    if (context.WebSockets.IsWebSocketRequest)
    {
        Console.WriteLine("WebSocket request detected");
    }
    await next();
});
// --- Standard environment setup ---
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();

}
else
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
    });
    
}
// --- Static + Blazor setup ---
app.UseStaticFiles();
if (!app.Environment.IsDevelopment())
    app.UseBlazorFrameworkFiles();

app.UseRouting();
app.UseCors("AllowClient");
app.UseAuthentication();
app.UseAuthorization();


// Map API controllers (will respect [Route("api/...")])
app.MapHub<ChatHub>("/api/chathub");
app.MapControllers();
// Blazor fallback
if (!app.Environment.IsDevelopment())
app.MapFallbackToFile("index.html");

app.Run();