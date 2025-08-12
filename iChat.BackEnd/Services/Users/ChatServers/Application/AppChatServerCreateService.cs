using Auth0.ManagementApi.Models;
using iChat.BackEnd.Services.Users.ChatServers.Abstractions;
using iChat.BackEnd.Services.Users.ChatServers.Abstractions.Cache.ChatServer;
using iChat.BackEnd.Services.Users.ChatServers.Abstractions.DB;
using iChat.DTOs.ChatServerDatas;
using iChat.DTOs.Collections;
using iChat.DTOs.Shared;
using iChat.DTOs.Users.Messages;

namespace iChat.BackEnd.Services.Users.ChatServers.Application
{
    public class AppChatServerCreateService
    {

        private readonly IChatCreateDBService createService;
        private readonly IChatServerRepository chatServerRepository;
        private readonly IChannelRepository channelRepository;
        private readonly AppChatServerService appChatServerService;
        private readonly IPermissionService permissionService;
        private readonly IServerUserRepository serverUserRepository;
        public AppChatServerCreateService(IChatCreateDBService createService,
            IChatServerRepository serverMetaDataCacheService,
            AppChatServerService appChatServerService,
            IChannelRepository channelMetaDataRepository,
            IPermissionService permissionService,
            IServerUserRepository serverUserRepository)
        {
            channelRepository = channelMetaDataRepository;
            this.permissionService = permissionService;
            this.createService = createService;
            this.chatServerRepository = serverMetaDataCacheService;
            this.appChatServerService = appChatServerService;
            this.serverUserRepository = serverUserRepository;
        }
        public async Task<OperationResultT<ChatServerData>> CreateServerAsync(ChatServerCreateRq request, long userId)
        {
            
            if (string.IsNullOrWhiteSpace(request.Name))
            {
                return OperationResultT<ChatServerData>.Fail("400", "Server name cannot be empty.");
            }
            var server = await createService.CreateServerAsync(request.Name, userId);
             chatServerRepository.UploadNewServerAsync(server);
            serverUserRepository.UploadNewServerAsync(server);
            await appChatServerService.Join(new UserId(userId), server.Id);
            return OperationResultT<ChatServerData>.Ok(server);

        }
        public async Task<OperationResultT<ChatChannelDto>> CreateChannelAsync(ChatChannelCreateRq request, UserId userId)
        {
            var serverId = new ServerId(new stringlong (request.ServerId));
            

            if (string.IsNullOrWhiteSpace(request.Name))
            {
                return OperationResultT<ChatChannelDto>.Fail("400", "Channel name cannot be empty.");
            }
            var permResult = permissionService.IsAdmin(serverId, userId);
            if (!permResult.Success)
            {
                return OperationResultT<ChatChannelDto>.Fail("500", "Failed to check admin permission.");
            }
            if (!permResult.Value)
            {
                return OperationResultT<ChatChannelDto>.Fail("403", "User is not an admin of the server.");
            }
            var result = await createService.CreateChannelAsync(serverId, request.Name, userId);
             channelRepository.AddChannel(result);
            return OperationResultT<ChatChannelDto>.Ok(result);
        }

    }
}
