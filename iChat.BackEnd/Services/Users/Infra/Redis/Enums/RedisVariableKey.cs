using Key = iChat.BackEnd.Services.Users.Infra.Redis.RedisKeyTable;
namespace iChat.BackEnd.Services.Users.Infra.Redis.Enums
{
    public static class RedisVariableKey
    {
        public static string GetUserServerKey(string userId)
        {
            return $"{Key.User}:{userId}:{Key.ChatServer}";
        }
        public static string GetUserServerKey_Lock(string userId)
        {
            return GetServerChannelKey(userId) + "_l";
        }
        public static string GetServerChannelKey(long serverId)
        {
            return $"{Key.ChatServer}:{serverId}:{Key.ChatChannel}";
        }
        public static string GetServerChannelKey(string serverId)
        {
            return $"{Key.ChatServer}:{serverId}:{Key.ChatChannel}";
        }
        public static string GetServerChannelKey_Lock(string serverId)
        {
            return GetServerChannelKey(serverId) + "_l";
        }
        public static string GetRecentChatMessageKey(string channelId)
        {
            return $"{Key.ChatChannel}:{channelId}:{Key.Message}";
        }
        public static string GetRecentChatMessageKey_Lock(string channelId)
        {
            return GetRecentChatMessageKey(channelId) + "_l";
        }
    }
}
