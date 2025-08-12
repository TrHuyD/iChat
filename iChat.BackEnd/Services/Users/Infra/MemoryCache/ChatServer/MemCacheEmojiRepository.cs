using iChat.BackEnd.Services.Users.ChatServers.Abstractions.Cache.ChatServer;
using iChat.DTOs.Collections;
using iChat.DTOs.Shared;
using iChat.DTOs.Users.Servers;

namespace iChat.BackEnd.Services.Users.Infra.MemoryCache.ChatServer
{
    public class MemCacheEmojiRepository : IEmojiRepository
    {
        private readonly IChatServerRepository _serverRepository;

        public MemCacheEmojiRepository(IChatServerRepository serverRepository)
        {
            _serverRepository = serverRepository;
        }
        public bool AddEmojiAsync(ServerId serverId, EmojiBaseDto emoji)
        {
            var serverResult =  _serverRepository.GetServerAsync(serverId, false);
            if (!serverResult.Success)
                return false;
            serverResult.Value.Emojis.Add(emoji);
            return true;
        }
        public  bool RemoveEmojiAsync(ServerId serverId, stringlong emojiId)
        {
            var serverResult =  _serverRepository.GetServerAsync(serverId, false);
            if (!serverResult.Success)
                return false;

            if (serverResult.Value.Emojis == null)
                return false;

            var removed = serverResult.Value.Emojis.RemoveAll(e => e.Id == emojiId) > 0;
            return removed;
        }

        public  OperationResultT<List<EmojiBaseDto>> GetEmojisAsync(ServerId serverId)
        {
            var serverResult =  _serverRepository.GetServerAsync(serverId, true);
            if (serverResult.Failure)
                return serverResult.FailAs<List<EmojiBaseDto>>();
            return OperationResultT<List<EmojiBaseDto>>.Ok(serverResult.Value.Emojis);
        }

        //public async Task<bool> UpdateEmojiAsync(Id serverId, EmojiDto emoji)
        //{
        //    var serverResult = await _serverRepository.GetServerAsync(serverId, false);
        //    if (!serverResult.Success || serverResult.Value.Emojis == null)
        //        return false;

        //    var existingEmoji = serverResult.Value.Emojis.FirstOrDefault(e => e.Id == emoji.Id);
        //    if (existingEmoji == null)
        //        return false;

        //    var index = serverResult.Value.Emojis.IndexOf(existingEmoji);
        //    serverResult.Value.Emojis[index] = emoji;
        //    return true;
        //}
    }
}
