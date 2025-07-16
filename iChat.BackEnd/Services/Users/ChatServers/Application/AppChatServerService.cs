using iChat.BackEnd.Services.Users.ChatServers.Abstractions;
using iChat.BackEnd.Services.Users.ChatServers.Abstractions.DB;
using iChat.BackEnd.Services.Users.Infra.MemoryCache;
using iChat.DTOs.Users.Messages;

namespace iChat.BackEnd.Services.Users.ChatServers.Application
{
    public class AppChatServerService
    {
        IChatServerDbService _dbService;
        IChatServerMetadataCacheService _localMem;
        public AppChatServerService(IChatServerDbService dbService, IChatServerMetadataCacheService localMem)
        {
            _dbService = dbService; 
            _localMem = localMem;
        }
        public async Task Join(long userId, long serverId)
        {
            await _dbService.Join(userId, serverId);
            _localMem.AddUserToServer(userId, serverId,true);
        }
        public async Task<ChatServerMetadata> EditServerName(long userId, long serverId)
        {
            throw new NotImplementedException();
        }

    }
}
