using iChat.BackEnd.Services.StartUpServices.SUS_ChatServer;
using iChat.BackEnd.Services.Users.ChatServers.Abstractions;
using iChat.BackEnd.Services.Users.ChatServers.Abstractions.Cache.ChatServer;
using iChat.BackEnd.Services.Users.Infra.Redis.ChatServerServices;
using iChat.Data.EF;
using Microsoft.Extensions.Hosting;

public class SUS_ServerChannelCacheLoader : IHostedService
{
    private readonly IServiceProvider _serviceProvider;

    public SUS_ServerChannelCacheLoader(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        Console.WriteLine("[SUS] Server Channel Cache Loader started.");

        // Resolve scoped services
        using var scope = _serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<iChatDbContext>();
        var cache = scope.ServiceProvider.GetRequiredService<IChatServerRepository>();
        var userService = scope.ServiceProvider.GetRequiredService<
        IServerUserRepository>();
        var serverLister = new SUS_EfCoreServerLister(db);
        var allServers = await serverLister.GetAllServersWithChannelsAsync();

        Console.WriteLine($"[SUS] Amount of Server retrieved from EF Core: {allServers.Count}");
        var result =  cache.UploadServersAsync(allServers);
        userService.UploadServersAsync(allServers);
        Console.WriteLine($"[SUS] Server Channel Cache Loader completed. Result: {result}");
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
