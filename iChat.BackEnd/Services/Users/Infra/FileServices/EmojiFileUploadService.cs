using iChat.BackEnd.Models.ChatServer;
using iChat.BackEnd.Models.Helpers;
using iChat.BackEnd.Services.Users.ChatServers.Abstractions.DB;
using iChat.BackEnd.Services.Users.Infra.IdGenerator;
using iChat.Data.EF;
using iChat.Data.Entities.Users.Messages;
using iChat.DTOs.Collections;
using iChat.DTOs.Shared;
using iChat.DTOs.Users.Servers;

namespace iChat.BackEnd.Services.Users.Infra.FileServices
{
    public class EmojiFileUploadService : IEmojiFileUploadService
    {
        private readonly IMediaUploadService _mediaUploadService;
        private readonly iChatDbContext _db;
        private readonly EmojiIdService _idService;
        public EmojiFileUploadService(IMediaUploadService mediaUploadService,iChatDbContext db,EmojiIdService idService)
        {
            _mediaUploadService = mediaUploadService;
            _db = db;
            _idService = idService;
        }

        public async Task<OperationResultT< EmojiDetailDto>> SaveEmojiAsync(CompleteEmojiRequest request)
        {
            var snowflakedto = _idService.GenerateId();
            string fileName = $"{snowflakedto.Id}.webp";

            try
            {
                var uploadResult = await _mediaUploadService.SaveImageWithProvidedIdAsync(
                request.File,
                request.UserId,
                ImageUploadProfiles.Emoji,
                fileName
                );
                var name = Path.GetFileNameWithoutExtension(request.File.FileName);
                if (name.Length == 0 || name.Length >= 16)
                    name = "__";
                var emoji = new Emoji
                {
                    Id = snowflakedto.Id,
                    Name = name,
                    ServerId = request.ServerId
                };
                _db.Emojis.Add(emoji);
                await _mediaUploadService.CommitAsync();
                return OperationResultT<EmojiDetailDto>.Ok(emoji.ToDetailDto());
            }
            catch(Exception ex)
            {
                return OperationResultT<EmojiDetailDto>.Fail("400", ex.Message);
            }
          
        }
    }
}
