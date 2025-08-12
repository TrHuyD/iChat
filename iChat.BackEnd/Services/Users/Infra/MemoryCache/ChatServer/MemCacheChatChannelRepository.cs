using iChat.BackEnd.Services.Users.ChatServers.Abstractions.Cache.ChatServer;
using iChat.DTOs.Users.Messages;

namespace iChat.BackEnd.Services.Users.Infra.MemoryCache.ChatServer
{
    public class MemCacheChatChannelRepository : IChannelRepository
    {
        private readonly IChatServerRepository _serverRepository;

        public MemCacheChatChannelRepository(IChatServerRepository serverRepository)
        {
            _serverRepository = serverRepository;
        }
        public bool AddChannel(ChatChannelDto channel)
        {
            var metadata = _serverRepository.GetServerAsync(channel.ServerId, false);
            if (!metadata.Success)
                return false;
            metadata.Value.Channels.Add(channel);
            return true;
        }
    }
}
