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
using iChat.BackEnd.Services.Users.Infra.Neo4j;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Configuration.AddUserSecrets<Program>();
var neo4jConfig = builder.Configuration.GetSection("Neo4j");
var CassandraRWConfig = new CassandraOptions(builder.Configuration.GetSection("Cassandra:ReadWriteOnly"));
builder.Services.AddSingleton<IDriver>(GraphDatabase.Driver(neo4jConfig["Uri"], AuthTokens.Basic(neo4jConfig["Username"], neo4jConfig["Password"]!)));
builder.Services.AddTransient<IAsyncSession>(provider =>
{
    var driver = provider.GetRequiredService<IDriver>();
    return driver.AsyncSession();
});

builder.Services.AddSingleton(new MessageUpdateService(CassandraRWConfig));


builder.Services.AddTransient<ChatChannelService>();
builder.Services.AddTransient<ChatServerService>();
builder.Services.AddTransient<UserRelationService>();
// Database Context
builder.Services.AddDbContext<iChatDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("iChatdev")));

// Identity Configuration with Cookie Authentication
builder.Services.AddIdentity<AppUser, Role>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
})
    .AddEntityFrameworkStores<iChatDbContext>()
    .AddDefaultTokenProviders();

// Configure Cookie Authentication
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Login";
    options.LogoutPath = "/Logout";
    options.AccessDeniedPath = "/Forbidden";
    options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
    options.SlidingExpiration = true;
});
builder.Services.AddTransient<IUserService, UserService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IPublicUserService, PublicUserService>();
builder.Services.AddHttpContextAccessor();

// Configure Authentication
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie();

// Configure Logging
builder.Services.AddLogging(logging =>
{
    logging.ClearProviders();
    logging.AddConsole();
    logging.SetMinimumLevel(LogLevel.Information);
});

var app = builder.Build();

// Configure Middleware
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();