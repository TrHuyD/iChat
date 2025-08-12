using iChat.BackEnd.Services.Users.ChatServers.Abstractions;
using iChat.BackEnd.Services.Users.ChatServers.Abstractions.Cache.ChatServer;
using iChat.DTOs.Collections;
using iChat.DTOs.Users;
using iChat.DTOs.Users.Messages;
namespace iChat.BackEnd.Services.Users.ChatServers.Application
{
    public class AppChatServerCacheService
    {
        IServerUserRepository _localMem;
        IPermissionService _permissionService;
        IChatServerRepository _chatServerRepository;
        ChatHubResponer _chatHubResponer;
        public AppChatServerCacheService(IServerUserRepository _serverUserCache
            , ChatHubResponer chatHubResponer,
            IPermissionService permissionService,
            IChatServerRepository chatServerRepository
            ) {
            _chatServerRepository = chatServerRepository;
            _permissionService = permissionService;
            _localMem = _serverUserCache;_chatHubResponer = chatHubResponer; }
        public async Task SetUserOnline(UserMetadata userId, List<long> serverId)
        {

            var (success, _, _) = _localMem.SetUserOnline(serverId, userId);
            if (success)
                _ = _chatHubResponer.BroadcastUserOnline( userId.userId, serverId);
        }
        public async Task SetUserOffline(UserId userId)
        {
            var (success, serverList, _, _) = _localMem.SetUserOffline(userId);
            if (success)
                _ = _chatHubResponer.BroadcastUserOffline(userId, serverList);
        }
        public async Task JoinNewServer(UserId userId, ServerId serverId)
        {
            var result = _localMem.AddUserToServer(userId, serverId);
            _=_chatHubResponer.BroadcastNewUser(userId.ToString(),serverId.ToString(),result.isOnline);
        
        }
        public async Task<bool> IsMember(ServerId serverId,ChannelId channelId,UserId userId)
        {
            var result= _permissionService.IsAdmin(serverId, channelId, userId);
            if (result.Success)
                return true;
            return false;
        }
        public async Task<MemberList> GetMemberList(ServerId serverId, int amount = 50, int skip = 0)
        {
            var(online,offline) = _localMem.GetUserList(serverId);
            return new MemberList { online= online,offline=offline ,serverId=serverId};
        }
        public async Task<bool> IsAdmin(ServerId serverId,UserId userId)
        {
            var result = _permissionService.IsAdmin(serverId, userId);
            if (result.Success)
                return result.Value;
            return false;
        }
        public async Task UpdateServerChange(ChatServerChangeUpdate server)
        {
            var result = _chatServerRepository.UpdateServerMetadata(server);
            _ = _chatHubResponer.ServerProfileChange(server);
        }

    }
}
