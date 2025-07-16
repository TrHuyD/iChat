using iChat.BackEnd.Services.Users.ChatServers.Abstractions;
using iChat.BackEnd.Services.Users.ChatServers.Abstractions.DB;
using iChat.DTOs.Users.Messages;

namespace iChat.BackEnd.Services.Users.ChatServers.Application
{
    public class AppChatServerCreateService
    {

        private readonly IChatCreateDBService createService;
        private readonly IChatServerMetadataCacheService serverMetaDataCacheService;
        private readonly AppChatServerService appChatServerService;
        public AppChatServerCreateService(IChatCreateDBService createService, 
            IChatServerMetadataCacheService serverMetaDataCacheService,
            AppChatServerService appChatServerService)
        {
            this.createService = createService;
            this.serverMetaDataCacheService = serverMetaDataCacheService;
            this.appChatServerService = appChatServerService;
        }
        public async Task<ChatServerMetadata> CreateServerAsync(ChatServerCreateRq request, long userId)
        {
            
            if (string.IsNullOrWhiteSpace(request.Name))
            {
                throw new ArgumentException("Server name cannot be empty.", nameof(request.Name));
            }
            var server = await createService.CreateServerAsync(request.Name, userId);
            serverMetaDataCacheService.UploadServerAsync(server);
            await appChatServerService.Join(userId, long.Parse(server.Id));
            return server;

        }
        public async Task<ChatChannelDto> CreateChannelAsync(ChatChannelCreateRq request, long userId)
        {
            var serverId = long.Parse(request.ServerId);
            if (string.IsNullOrWhiteSpace(request.Name))
            {
                throw new ArgumentException("Server name cannot be empty.", nameof(request.Name));
            }
            if(!await serverMetaDataCacheService.IsAdmin(serverId, userId))
            {
                throw new UnauthorizedAccessException("User is not admin in cache");
            }
            var result= await createService.CreateChannelAsync(serverId,request.Name,userId);
            serverMetaDataCacheService.AddChannelAsync(result);
            return result;
        }
    }
}
