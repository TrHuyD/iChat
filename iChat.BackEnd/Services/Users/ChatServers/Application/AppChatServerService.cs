﻿using iChat.BackEnd.Services.Users.ChatServers.Abstractions;
using iChat.BackEnd.Services.Users.ChatServers.Abstractions.DB;
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
        Lazy<IMediaUploadService> _imageUploader;
        public AppChatServerService(IChatServerDbService dbService, AppChatServerCacheService chatServerDbService,Lazy<IMediaUploadService> imageUploader)
        {
            _imageUploader = imageUploader;
            _dbService = dbService;
            this._localMem = chatServerDbService;
        }
        public async Task Join(long userId, long serverId)
        {
            await _dbService.Join(userId, serverId);
            await _localMem.JoinNewServer(userId, serverId);
        }
        public async Task<OperationResultT<ChatServerChangeUpdate>> EditServerProfile(stringlong userId, stringlong serverId, string newName="", IFormFile file=null)
        {

            if (await _localMem.IsAdmin(serverId, userId))
            {
                var newAvatarUrl = "";
                if (file != null)
                    newAvatarUrl = (await _imageUploader.Value.SaveAvatarAsync(file, userId)).Url;
                var result = await _dbService.UpdateChatServerProfileAsync(serverId, userId, newName, newAvatarUrl );
                await _localMem.UpdateServerChange(result);
                return OperationResultT<ChatServerChangeUpdate>.Ok(result);
            }

            return OperationResultT<ChatServerChangeUpdate>.Fail("400", $"User {userId} doesn't have permission to edit profile of server {serverId}");
        }

    }
}
