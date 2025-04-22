using iChat.BackEnd.Services.StartUpServices.SUS_ChatServer;
using iChat.BackEnd.Services.Users.Infra.Neo4jService;
using iChat.BackEnd.Services.Users.Infra.Redis.ChatServerServices;
using Neo4j.Driver;
using System.Threading.Tasks;

public class SUS_ServerChannelCacheLoader : IHostedService
{
    private readonly Lazy<IAsyncSession> _neo4jSession;
    private readonly RedisChatServerService _redisChatServerService;

    public SUS_ServerChannelCacheLoader(
        Lazy<IAsyncSession> neo4jSession,
        RedisChatServerService redisChatServerService)
    {
        _neo4jSession = neo4jSession;
        _redisChatServerService = redisChatServerService;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        Console.WriteLine("[SUS] Server Channel Cache Loader started.");
        var serverLister = new SUS_Neo4jServerLister(_neo4jSession);
        var allServers = await serverLister.GetAllServerChannelsAsync();
        Console.WriteLine($"[SUS] Amount of Server retrieve from Neo4j: {allServers.Count}");
        var result = await _redisChatServerService.UploadServerAsync(allServers);
        Console.WriteLine($"[SUS] Server Channel Cache Loader completed. Result: {result}");
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
