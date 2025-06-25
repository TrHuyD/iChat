using StackExchange.Redis;
using Key = iChat.BackEnd.Services.Users.Infra.Redis.RedisKeyTable;
namespace iChat.BackEnd.Services.Users.Infra.Redis.Enums
{
    public static class RedisVariableKey
    {
        internal static string GetUserServerKey(long userId)
        {
            return $"{Key.User}:{userId}:{Key.ChatServer}";
        }
        internal static string GetUserServerKey_Lock(long userId)
        {
            return GetServerChannelKey(userId) + "_l";
        }
        public static string GetServerMetadataKey(string serverId) => $"server:{serverId}:meta";
        public static string GetServerChannelsKey(string serverId) => $"server:{serverId}:channels";
        public static string GetChannelBucketKey(string serverId, string channelId) =>
            $"server:{serverId}:channels";
        internal static string GetServerChannelKey(long serverId)
        {
            return $"{Key.ChatServer}:{serverId}:{Key.ChatChannel}";
        }
        internal static string GetServerChannelKey_Lock(long serverId)
        {
            return GetServerChannelKey(serverId) + "_l";
        }
        internal static string GetRecentChatMessageKey(long channelId)
        {
            return $"{Key.ChatChannel}:{channelId}:{Key.Message}";
        }
        internal static string GetRecentChatMessageKey_Lock(long channelId)
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
