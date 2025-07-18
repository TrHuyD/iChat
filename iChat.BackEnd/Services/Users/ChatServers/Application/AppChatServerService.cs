using iChat.BackEnd.Services.Users.ChatServers.Abstractions;
using iChat.BackEnd.Services.Users.ChatServers.Abstractions.DB;
using iChat.BackEnd.Services.Users.Infra.MemoryCache;
using iChat.DTOs.Users;
using iChat.DTOs.Users.Messages;

namespace iChat.BackEnd.Services.Users.ChatServers.Application
{
    public class AppChatServerService
    {
        IChatServerDbService _dbService;
        AppChatServerCacheService _localMem;
        public AppChatServerService(IChatServerDbService dbService, AppChatServerCacheService chatServerDbService)
        {
            _dbService = dbService;
            this._localMem = chatServerDbService;
        }
        public async Task Join(long userId, long serverId)
        {
            await _dbService.Join(userId, serverId);
            await _localMem.JoinNewServer(userId, serverId);
        }

        
        public async Task<ChatServerMetadata> EditServerName(long userId, long serverId)
        {
            throw new NotImplementedException();
        }

    }
}
