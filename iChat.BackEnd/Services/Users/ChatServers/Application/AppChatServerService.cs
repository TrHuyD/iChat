﻿using iChat.BackEnd.Services.Users.ChatServers.Abstractions.DB;
using iChat.BackEnd.Services.Users.Infra.MemoryCache;

namespace iChat.BackEnd.Services.Users.ChatServers.Application
{
    public class AppChatServerService
    {
        IChatServerDbService _dbService;
        MemCacheUserChatService _localMem;
        public AppChatServerService(IChatServerDbService dbService, MemCacheUserChatService localMem)
        {
            _dbService = dbService; 
            _localMem = localMem;
        }
        public async Task Join(long userId, long serverId)
        {
            await _dbService.Join(userId, serverId);
            _localMem.AddServerToUser(userId, serverId);
        }

    }
}
