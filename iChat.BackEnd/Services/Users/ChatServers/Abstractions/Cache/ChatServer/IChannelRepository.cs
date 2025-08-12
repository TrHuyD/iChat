using iChat.DTOs.Users.Messages;

namespace iChat.BackEnd.Services.Users.ChatServers.Abstractions.Cache.ChatServer
{
    public interface IChannelRepository
    {
       bool AddChannel(ChatChannelDto channel);
    }
}
