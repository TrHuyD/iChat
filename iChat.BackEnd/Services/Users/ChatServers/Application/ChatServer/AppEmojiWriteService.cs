using iChat.BackEnd.Models.ChatServer;
using iChat.BackEnd.Services.Users.ChatServers.Abstractions.Cache.ChatServer;
using iChat.BackEnd.Services.Users.Infra.FileServices;
using iChat.DTOs.Shared;
using System.Threading.Tasks;

namespace iChat.BackEnd.Services.Users.ChatServers.Application.ChatServer
{
    public class AppEmojiWriteService 
    {
        private readonly IEmojiFileUploadService _emojiDbService;
        private readonly IEmojiRepository _emojiRepository;
        private readonly IPermissionService _permissionService;
        public AppEmojiWriteService(IEmojiFileUploadService emojiDbService, IEmojiRepository emojiRepository, IPermissionService permissionService  )
        {
            _emojiDbService = emojiDbService;
            _emojiRepository = emojiRepository;
            _permissionService = permissionService;
        }
        public  async Task<OperationResult> Create(CompleteEmojiRequest request )
        {
            var per_result =_permissionService.IsAdmin(request.ServerId,request.UserId);    
            if(per_result.Failure)
                return OperationResult.Fail("400", per_result.ErrorMessage);
            var result = await _emojiDbService.SaveEmojiAsync(request );
            if(result.Failure)
            {
                return OperationResult.Fail("400",result.ErrorMessage);
            }
            _emojiRepository.AddEmojiAsync(request.ServerId, result.Value.GetBase());
            return OperationResult.Ok();
        }
    }
}
