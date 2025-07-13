using iChat.BackEnd.Models.Helpers;
using iChat.BackEnd.Services.Users;
using iChat.BackEnd.Services.Users.Auth;
using iChat.Data.EF;
using iChat.Data.Entities.Users;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

using iChat.BackEnd.Services.Users.Infra.IdGenerator;
using iChat.BackEnd.Models.Infrastructures;
using Microsoft.OpenApi.Models;

using iChat.BackEnd.Services.Users.Infra.Redis;
using iChat.BackEnd.Services.Users.Infra.Helpers;
using iChat.BackEnd.Services.Validators;
using iChat.BackEnd.Services.Users.ChatServers;
using iChat.BackEnd.Services.Users.Infra.Redis.MessageServices;
using iChat.BackEnd.Services.Users.Infra.Redis.ChatServerServices;
using iChat.BackEnd.Services.Users.Auth.Sql;
using System.Text.Json;
using iChat.BackEnd.Controllers.UserControllers.MessageControllers;
using iChat.BackEnd.Services.Users.ChatServers.Abstractions;
using iChat.BackEnd.Services.Users.Infra.EfCore.MessageServices;
using iChat.BackEnd.Services.Users.Infra.EFcore.MessageServices;
using iChat.BackEnd.Services.Users.Infra.MemoryCache;

var builder = WebApplication.CreateBuilder(args);

// Configure environment-specific settings
if (builder.Environment.IsDevelopment())
{
    builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new OpenApiInfo { Title = "iChat API", Version = "v1" });
    });

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
    builder.Configuration.AddJsonFile("appsettings.production.json");
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowClient",
           builder => builder.WithOrigins("https://trhuyd.github.io")
                             .AllowAnyMethod()
                             .AllowAnyHeader()
                             .AllowCredentials());
    });
}

// Add configuration sources
builder.Configuration.AddJsonFile("appsettings.secrets.json");
builder.Configuration.AddUserSecrets<Program>();

// Add controllers
builder.Services.AddControllers();
builder.Services.AddHttpContextAccessor();

// Configure Worker ID
var WorkerIdConfig = new ConfigurationBuilder()
    .AddJsonFile("workerSettings.json", optional: true, reloadOnChange: true)
    .Build().GetSection("Id").Get<WorkerID>();

// Add core services
builder.Services.AddSingleton<ThreadSafeCacheService>();
builder.Services.AddSingleton<IRedisConnectionService>(provider =>
{
    var configuration = provider.GetRequiredService<IConfiguration>();
    return new CloudRedisCacheService(configuration);
});
builder.Services.AddMemoryCache();
builder.Services.AddSingleton<AppRedisService>();
builder.Services.AddSingleton<RedisLiveTime>();
builder.Services.AddSingleton<RedisChatServerService>();

// Add helper services
new IdBuilderHelper().AddService(builder, WorkerIdConfig);
new ValidatorsHelper(builder);

// Add business services
builder.Services.AddTransient<IChatServerEditService, EfCoreChatServerEditService>();
builder.Services.AddScoped<IChatListingService, EfCoreChatListingService>();
builder.Services.AddScoped<IMessageDbReadService, EfCoreMessageReadService>();
builder.Services.AddScoped<IMessageDbWriteService, EfCoreMessageWriteService>();
builder.Services.AddSingleton<MessageTimeLogger>();
builder.Services.AddSingleton<MessageWriteQueueService>();
builder.Services.AddHostedService<BucketingPrediodicService>();
builder.Services.AddHostedService(sp => sp.GetRequiredService<MessageWriteQueueService>());

// Add Redis services
builder.Services.AddTransient<RedisUserServerService>();
builder.Services.AddTransient<RedisChatCache>();
builder.Services.AddTransient<RedisSegmentCache>();
builder.Services.AddTransient<MemCacheUserChatService>();

// Add server services
builder.Services.AddTransient<ServerListService>();
builder.Services.AddTransient<IMessageWriteService, Test_UserSendTextMessageService>();
builder.Services.AddTransient<IChatReadMessageService, Test_UserChatReadMessageService>();

// Database Context
builder.Services.AddDbContext<iChatDbContext>(
    options =>
        options.UseNpgsql(builder.Configuration.GetValue<string>("PostgreSQL:ConnectionString"))
               .LogTo(Console.WriteLine, LogLevel.Information),
    ServiceLifetime.Scoped
);

// Identity Configuration
builder.Services.AddIdentity<AppUser, Role>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
})
    .AddEntityFrameworkStores<iChatDbContext>()
    .AddDefaultTokenProviders();

// Add authentication services
new SqlAuthBuilderHelper().AddService(builder);

// Add user and chat services
builder.Services.AddScoped<IChatCreateService, EfCoreChatCreateService>();
builder.Services.AddScoped<CreateUserService>();
builder.Services.AddTransient<IUserService, EfcoreUserService>();
builder.Services.AddScoped<IPublicUserService, PublicUserService>();
builder.Services.AddScoped<IMessageLastSeenService, RedisMessageLastSeenService>();
builder.Services.AddScoped<IUserPresenceCacheService, MemCacheUserPresence>();
builder.Services.AddScoped<IChatServerService, EfCoreChatServerService>();
builder.Services.AddScoped<RedisCSInviteLinkService>();
builder.Services.AddScoped<UserMetadataService>();
builder.Services.AddScoped<IUserMetaDataCacheService, UserMetadataRedisCacheService>();
builder.Services.AddTransient<Lazy<IUserService>>(provider => new Lazy<IUserService>(() => provider.GetRequiredService<IUserService>()));
builder.Services.AddSingleton<IChatServerMetadataCacheService, MemCacheChatServerMetadataService>();
builder.Services.AddScoped<IMessageReadService, MessageReadApplicationService>();
builder.Services.AddScoped<IMessageCacheService,MemCacheMessageService>();
builder.Services.AddTransient<ChatHubResponer, ChatHubResponer>();
// Add hosted services
builder.Services.AddHostedService<SUS_ServerChannelCacheLoader>();
builder.Services.AddScoped<IMessageSearchService, EfCoreMessageSearchService>();

// Add API documentation
builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();

// Request logging middleware
app.Use(async (context, next) =>
{
    var path = context.Request.Path;
    Console.WriteLine($"IN->Request: {context.Request.Method} {path}");

    await next();

    var type = context.Response.ContentType ?? "none";
    Console.WriteLine($"OUT<- Response: {context.Response.StatusCode} ({type})");
});

// WebSocket detection middleware
app.Use(async (context, next) =>
{
    if (context.WebSockets.IsWebSocketRequest)
    {
        Console.WriteLine("WebSocket request detected");
    }
    await next();
});

// Environment-specific configuration
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "iChat API V1");
    });
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// Configure middleware pipeline
app.UseRouting();
app.UseCors("AllowClient");
app.UseAuthentication();
app.UseAuthorization();

// Map API routes
app.MapHub<ChatHub>("/api/chathub");
app.MapControllers();

// Handle unmatched API routes
app.MapGet("/api/{**catch-all}", async context =>
{
    context.Response.StatusCode = 404;
    context.Response.ContentType = "application/json";
    await context.Response.WriteAsync(JsonSerializer.Serialize(new
    {
        error = "Not Found",
        message = "API endpoint not found"
    }));
});

// Handle root requests
app.MapGet("/", async context =>
{
    context.Response.ContentType = "application/json";
    await context.Response.WriteAsync(JsonSerializer.Serialize(new
    {
        message = "iChat API is running",
        version = "1.0",
        timestamp = DateTime.UtcNow
    }));
});

app.Run();