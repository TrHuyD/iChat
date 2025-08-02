using iChat.BackEnd.Services.Users.ChatServers.Abstractions;
using iChat.DTOs.Collections;
using iChat.DTOs.Users;
using iChat.DTOs.Users.Messages;
namespace iChat.BackEnd.Services.Users.ChatServers.Application
{
    public class AppChatServerCacheService
    {
        IChatServerMetadataCacheService _localMem;
        ChatHubResponer _chatHubResponer;
        public AppChatServerCacheService(IChatServerMetadataCacheService localMem, ChatHubResponer chatHubResponer) { _localMem = localMem;_chatHubResponer = chatHubResponer; }
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
            var result=await _localMem.IsAdmin(serverId, channelId, userId);
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
            var result =  _localMem.IsAdmin(serverId, userId);
            if (result.Success)
                return result.Value;
            return false;
        }
        public async Task UpdateServerChange(ChatServerChangeUpdate server)
        {
            var result =await _localMem.UpdateServerMetadata(server);
            _ = _chatHubResponer.ServerProfileChange(server);
        }

    }
}
