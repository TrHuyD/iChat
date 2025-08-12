using iChat.BackEnd.Models.ChatServer;
using iChat.BackEnd.Services.Users.Infra.IdGenerator;
using iChat.Data.Entities.Users.Messages;
using iChat.DTOs.Collections;
using iChat.DTOs.Shared;
using iChat.DTOs.Users.Servers;

namespace iChat.BackEnd.Services.Users.Infra.FileServices
{
    public interface IEmojiFileUploadService
    {
        Task<OperationResultT<EmojiDetailDto>> SaveEmojiAsync(CompleteEmojiRequest request);
    }
}
