using iChat.DTOs.Collections;
using iChat.DTOs.Shared;

namespace iChat.BackEnd.Services.Users.ChatServers.Abstractions.Cache.ChatServer
{
    public interface IPermissionService
    {
        OperationResultBool IsAdmin(ServerId serverId, UserId userId);
        OperationResultBool IsAdmin(ServerId serverId, ChannelId channelId, UserId userId);
    }
}
