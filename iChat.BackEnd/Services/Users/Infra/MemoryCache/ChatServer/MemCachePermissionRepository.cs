using iChat.BackEnd.Services.Users.ChatServers.Abstractions.Cache.ChatServer;
using iChat.DTOs.Collections;
using iChat.DTOs.Shared;
using iChat.DTOs.Users.Servers;

namespace iChat.BackEnd.Services.Users.Infra.MemoryCache.ChatServer
{
    public class MemCachePermissionRepository : IPermissionService
    {
        private readonly IChatServerRepository _serverRepository;

        public MemCachePermissionRepository(IChatServerRepository serverRepository)
        {
            _serverRepository = serverRepository;
        }
        public OperationResultBool IsAdmin(ServerId serverId, UserId userId)
        {
            var serverResult =  _serverRepository.GetServerAsync(serverId, false);
            if (serverResult.Failure)
                return OperationResultBool.Fail("400", $"[UserService:IsAdmin] Server {serverId} not found in cache "+ serverResult.ErrorMessage);

            bool isAdmin = serverResult.Value.AdminId == userId;
            return OperationResultBool.Ok(isAdmin);
        }

        public OperationResultBool IsAdmin(ServerId serverId, ChannelId channelId, UserId userId)
        {
            var serverResult = _serverRepository.GetServerAsync(serverId, false);
            if (!serverResult.Success)
            {
                return OperationResultBool.Fail("400", $"[UserService:IsAdmin] Server {serverId} not found in cache + "+serverResult.ErrorMessage);
            }

            var server = serverResult.Value;
          
            if (!server.Channels.Any(c => c.Id == channelId.Value.ToString()))
            {
                return OperationResultBool.Fail("403", $"[UserService:IsAdmin] Channel {channelId} is not a channel of server {channelId}");
            }
            bool isAdmin = serverResult.Value.AdminId == userId;
            return OperationResultBool.Ok(isAdmin);
        }
    }
}
