using Auth0.ManagementApi.Models;
using iChat.BackEnd.Services.Users.ChatServers.Abstractions;
using iChat.BackEnd.Services.Users.ChatServers.Abstractions.DB;
using iChat.DTOs.Collections;
using iChat.DTOs.Shared;
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
        public async Task<OperationResultT<ChatServerMetadata>> CreateServerAsync(ChatServerCreateRq request, long userId)
        {
            
            if (string.IsNullOrWhiteSpace(request.Name))
            {
                return OperationResultT<ChatServerMetadata>.Fail("400", "Server name cannot be empty.");
            }
            var server = await createService.CreateServerAsync(request.Name, userId);
            await serverMetaDataCacheService.UploadNewServerAsync(server);
            await appChatServerService.Join(new UserId(userId), server.Id);
            return OperationResultT<ChatServerMetadata>.Ok(server);

        }
        public async Task<OperationResultT<ChatChannelDto>> CreateChannelAsync(ChatChannelCreateRq request, UserId userId)
        {
            var serverId = new ServerId(new stringlong (request.ServerId));
            

            if (string.IsNullOrWhiteSpace(request.Name))
            {
                return OperationResultT<ChatChannelDto>.Fail("400", "Channel name cannot be empty.");
            }
            var permResult = serverMetaDataCacheService.IsAdmin(serverId, userId);
            if (!permResult.Success)
            {
                return OperationResultT<ChatChannelDto>.Fail("500", "Failed to check admin permission.");
            }
            if (!permResult.Value)
            {
                return OperationResultT<ChatChannelDto>.Fail("403", "User is not an admin of the server.");
            }
            var result = await createService.CreateChannelAsync(serverId, request.Name, userId);
            await serverMetaDataCacheService.AddChannel(result);
            return OperationResultT<ChatChannelDto>.Ok(result);
        }

    }
}
