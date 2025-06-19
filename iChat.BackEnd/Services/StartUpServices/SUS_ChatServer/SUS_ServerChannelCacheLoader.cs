using iChat.Data.EF;
using iChat.BackEnd.Services.StartUpServices.SUS_ChatServer;
using iChat.BackEnd.Services.Users.Infra.Redis.ChatServerServices;
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
        var redisChatServerService = scope.ServiceProvider.GetRequiredService<RedisChatServerService>();

        var serverLister = new SUS_EfCoreServerLister(db);
        var allServers = await serverLister.GetAllServerChannelsAsync();

        Console.WriteLine($"[SUS] Amount of Server retrieved from EF Core: {allServers.Count}");
        var result = await redisChatServerService.UploadServerAsync(allServers);
        Console.WriteLine($"[SUS] Server Channel Cache Loader completed. Result: {result}");
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
