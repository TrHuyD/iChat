using StackExchange.Redis;
using Key = iChat.BackEnd.Services.Users.Infra.Redis.RedisKeyTable;
namespace iChat.BackEnd.Services.Users.Infra.Redis.Enums
{
    public static class RedisVariableKey
    {
        internal static string GetUserServerKey(string userId)
        {
            return $"{Key.User}:{userId}:{Key.ChatServer}";
        }
        internal static string GetUserServerKey_Lock(string userId)
        {
            return GetServerChannelKey(userId) + "_l";
        }
        internal static string GetServerChannelKey(long serverId)
        {
            return $"{Key.ChatServer}:{serverId}:{Key.ChatChannel}";
        }
        internal static string GetServerChannelKey(string serverId)
        {
            return $"{Key.ChatServer}:{serverId}:{Key.ChatChannel}";
        }
        internal static string GetServerChannelKey_Lock(string serverId)
        {
            return GetServerChannelKey(serverId) + "_l";
        }
        internal static string GetRecentChatMessageKey(string channelId)
        {
            return $"{Key.ChatChannel}:{channelId}:{Key.Message}";
        }
        internal static string GetRecentChatMessageKey_Lock(string channelId)
        {
            return GetRecentChatMessageKey(channelId) + "_l";
        }

        internal static string GetUserServerPermsKey(long userId, long serverId)
        {
           
            return $"{Key.User}:{userId}:{Key.ChatServer}:{serverId}:{Key.Perm}";
        }

        internal static string GetChannelUserPermsKey(long serverId, long channelId)
        {
            
            return $"{Key.ChatServer}:{serverId}:c:{channelId}:p";
        }

    }
}
