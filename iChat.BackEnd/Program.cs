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
using iChat.BackEnd.Services.Users.Servers;
using iChat.BackEnd.Services.Users.Infra.Helpers;

var builder = WebApplication.CreateBuilder(args);
if (builder.Environment.IsDevelopment())
{
    builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new OpenApiInfo { Title = "My API", Version = "v1" });
    });
    builder.Services.AddControllers();
}
// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Configuration.AddUserSecrets<Program>();
var neo4jConfig = builder.Configuration.GetSection("Neo4j");
var CassandraRWConfig = new CassandraOptions(builder.Configuration.GetSection("Cassandra:ReadWriteOnly"));
var WorkerIdConfig = new ConfigurationBuilder()
    .AddJsonFile("workerSettings.json", optional: true, reloadOnChange: true)
    .Build().GetSection("Id").Get<WorkerID>();
builder.Services.AddSingleton<ThreadSafeCacheService>();
builder.Services.AddSingleton<IRedisConnectionService>(provider =>
{
    var configuration = provider.GetRequiredService<IConfiguration>();
    return new CloudRedisCacheService(configuration);
});
builder.Services.AddScoped<AppRedisService>();
builder.Services.AddSingleton<RedisLiveTime>();
IdBuilderHelper.AddService(builder, WorkerIdConfig);

builder.Services.AddSingleton<IDriver>(GraphDatabase.Driver(neo4jConfig["Uri"], AuthTokens.Basic(neo4jConfig["Username"], neo4jConfig["Password"]!)));
builder.Services.AddTransient<IAsyncSession>(provider =>
{
    var driver = provider.GetRequiredService<IDriver>();
    return driver.AsyncSession();
});
builder.Services.AddSingleton(new MessageUpdateService(CassandraRWConfig));

builder.Services.AddTransient<Neo4jChatChannelEditService>();
builder.Services.AddTransient<Neo4jChatServerEditService>();
builder.Services.AddTransient(provider =>
    new Lazy<Neo4jChatListingService>(() => provider.GetRequiredService<Neo4jChatListingService>()));
builder.Services.AddTransient<UserRelationService>();
builder.Services.AddTransient<RedisUserServerService>();
builder.Services.AddTransient<ServerListService>();
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
builder.Services.AddTransient<CreateChatService>();
builder.Services.AddTransient < Neo4jCreateUserService>();
builder.Services.AddTransient<CreateUserService>();
builder.Services.AddTransient<IUserService, UserService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IPublicUserService, PublicUserService>();

builder.Services.AddEndpointsApiExplorer();


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
else
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
    });

}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
if(!app.Environment.IsDevelopment())
{

}
else
{
    app.UseEndpoints(endpoints =>
    {
        endpoints.MapControllers();
    });
}
    app.Run();