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
                _ = _chatHubResponer.BroadcastUserOnline(userId.UserId, serverId);
        }
        public async Task SetUserOffline(string userId)
        {
            var (success, serverList, _, _) = _localMem.SetUserOffline(userId);
            if (success)
                _ = _chatHubResponer.BroadcastUserOffline(userId, serverList);
        }
        public async Task JoinNewServer(long userId, long serverId)
        {
            var result = _localMem.AddUserToServer(userId, serverId);
            _=_chatHubResponer.BroadcastNewUser(userId.ToString(),serverId.ToString(),result.isOnline);
        
        }
        public async Task<MemberList> GetMemberList(stringlong serverId, int amount = 50, int skip = 0)
        {
            var(online,offline) = _localMem.GetUserList(serverId);
            return new MemberList { online= online,offline=offline ,serverId=serverId};
        }

    }
}
