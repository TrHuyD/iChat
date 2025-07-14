using iChat.DTOs.Users;
using iChat.DTOs.Users.Messages;

namespace iChat.BackEnd.Services.Users.ChatServers.Abstractions
{
    public interface IUserCacheChatService
    {
        #region Hot Path - Combined Operations (Most Frequent)

        /// <summary>
        /// Hot path: Gets both server membership and metadata version in one cache lookup
        /// Use this for message processing where you need both pieces of info
        /// </summary>
        (bool isInServer, long? metadataVersion) CheckUserAccessAndVersion(string userId, long serverId, bool extendExpire = false);

        /// <summary>
        /// Hot path: Get all data needed for message processing in one lookup
        /// </summary>
        MessageProcessingContext? GetMessageProcessingContext(string userId, long serverId, bool extendExpire = false);

        #endregion

        #region Online User Operations (Combined Data)

        /// <summary>
        /// Use when user comes online - cache both server list and metadata together
        /// </summary>
        void SetOnlineUserData(string userId, List<long> serverList, UserMetadata metadata);

        /// <summary>
        /// Update server list for online user - use when user joins/leaves servers
        /// </summary>
        bool UpdateOnlineUserServers(string userId, List<long> serverList);

        /// <summary>
        /// Add a server to user's server list (when user joins a new server)
        /// </summary>
        bool AddServerToUser(string userId, long serverId);

        /// <summary>
        /// Remove a server from user's server list (when user leaves a server)
        /// </summary>
        bool RemoveServerFromUser(string userId, long serverId);

        /// <summary>
        /// Update metadata for online user
        /// </summary>
        bool UpdateOnlineUserMetadata(string userId, UserMetadata metadata);

        #endregion

        #region Offline User Operations (Metadata Only)

        /// <summary>
        /// Use for offline users - cache only metadata with longer TTL
        /// </summary>
        void SetOfflineUserMetadata(string userId, UserMetadata metadata);

        /// <summary>
        /// Get metadata for offline user
        /// </summary>
        UserMetadata? GetMetadataOnly(string userId, bool extendExpire = false);

        #endregion

        #region Individual Operations (For Specific Cases)

        bool IsUserInServer(string userId, long serverId, bool extendExpire = false);

        long? GetMetadataVersion(string userId, bool extendExpire = false);

        UserMetadata? GetUserMetadata(string userId, bool extendExpire = false);

        /// <summary>
        /// Get user's server list (only available for online users)
        /// </summary>
        List<long>? GetUserServerList(string userId, bool extendExpire = false);

        /// <summary>
        /// Get all servers user is in as a HashSet for fast lookups (only available for online users)
        /// </summary>
        HashSet<long>? GetUserServerSet(string userId, bool extendExpire = false);

        /// <summary>
        /// Get count of servers user is in (only available for online users)
        /// </summary>
        int GetUserServerCount(string userId, bool extendExpire = false);

        /// <summary>
        /// Check if user is currently online (has combined cache entry)
        /// </summary>
        bool IsUserOnline(string userId);

        /// <summary>
        /// Check if user exists in cache (either online or offline)
        /// </summary>
        bool IsUserCached(string userId);

        #endregion

        #region Cache Management

        void InvalidateUser(string userId);

        /// <summary>
        /// Downgrade user's cache when they go offline - move from combined cache to metadata-only cache
        /// This saves memory by removing server list data while keeping metadata accessible
        /// </summary>
        void DowngradeUserCache(string userId);

        /// <summary>
        /// Alias for DowngradeUserCache - use when user goes offline
        /// </summary>
        void MoveUserToOffline(string userId);

        /// <summary>
        /// Batch invalidate multiple users efficiently
        /// </summary>
        void InvalidateUsers(IEnumerable<string> userIds);

        #endregion

        #region Backward Compatibility

        void SetUserChatData(string userId, List<long>? serverList = null, UserMetadata? metadata = null);

        (List<long>? serverList, UserMetadata? metadata) GetUserChatData(string userId, bool extendExpire = false);

        #endregion
    }
}
