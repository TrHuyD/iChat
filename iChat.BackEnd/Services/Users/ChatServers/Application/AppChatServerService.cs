using iChat.BackEnd.Services.Users.ChatServers.Abstractions;
using iChat.BackEnd.Services.Users.ChatServers.Abstractions.DB;
using iChat.BackEnd.Services.Users.Infra.FileServices;
using iChat.BackEnd.Services.Users.Infra.MemoryCache;
using iChat.DTOs.Collections;
using iChat.DTOs.Shared;
using iChat.DTOs.Users;
using iChat.DTOs.Users.Messages;

namespace iChat.BackEnd.Services.Users.ChatServers.Application
{
    public class AppChatServerService
    {
        IChatServerDbService _dbService;
        AppChatServerCacheService _localMem;
        IGenericMediaUploadService _imageUploader;
        public AppChatServerService(IChatServerDbService dbService, AppChatServerCacheService chatServerDbService, IGenericMediaUploadService imageUploader)
        {
            _imageUploader = imageUploader;
            _dbService = dbService;
            this._localMem = chatServerDbService;
        }
        public async Task Join(UserId userId, ServerId serverId)
        {
            await _dbService.Join(userId, serverId);
            await _localMem.JoinNewServer(userId, serverId);
        }
        public async Task<OperationResultT<ChatServerChangeUpdate>> EditServerProfile(UserId userId, ServerId serverId, string newName="", IFormFile file=null)
        {

            if (await _localMem.IsAdmin(serverId, userId))
            {
                var newAvatarUrl = "";
                if (file != null)
                    newAvatarUrl = (await _imageUploader.SaveAvatarAsync(file, userId)).Url;
                var result = await _dbService.UpdateChatServerProfileAsync(serverId, userId, newName, newAvatarUrl );
                await _localMem.UpdateServerChange(result);
                return OperationResultT<ChatServerChangeUpdate>.Ok(result);
            }

            return OperationResultT<ChatServerChangeUpdate>.Fail("400", $"User {userId} doesn't have permission to edit profile of server {serverId}");
        }

    }
}
