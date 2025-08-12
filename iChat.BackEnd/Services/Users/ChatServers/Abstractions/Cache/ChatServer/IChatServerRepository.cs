using iChat.BackEnd.Models.ChatServer;
using iChat.DTOs.ChatServerDatas;
using iChat.DTOs.Collections;
using iChat.DTOs.Shared;
using iChat.DTOs.Users.Messages;

namespace iChat.BackEnd.Services.Users.ChatServers.Abstractions.Cache.ChatServer
{
    public interface IChatServerRepository
    {
        bool UploadServersAsync(List<ChatServerbulk> servers);
        bool UploadNewServerAsync(ChatServerData server);
        OperationResultT<ChatServerData> GetServerAsync(ServerId serverId, bool isCopy = true);
        bool UpdateServerMetadata(ChatServerChangeUpdate update);
    }
}
