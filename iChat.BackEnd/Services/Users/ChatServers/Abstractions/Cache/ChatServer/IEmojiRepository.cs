using iChat.DTOs.Collections;
using iChat.DTOs.Shared;
using iChat.DTOs.Users.Servers;

namespace iChat.BackEnd.Services.Users.ChatServers.Abstractions.Cache.ChatServer
{
    public interface IEmojiRepository
    {
        bool AddEmojiAsync(ServerId serverId, EmojiBaseDto emoji);
        bool RemoveEmojiAsync(ServerId serverId, stringlong emojiId);
        OperationResultT<List<EmojiBaseDto>> GetEmojisAsync(ServerId serverId);
   //     Task<bool> UpdateEmojiAsync(Id serverId, EmojiBaseDto emoji);
    }
}
