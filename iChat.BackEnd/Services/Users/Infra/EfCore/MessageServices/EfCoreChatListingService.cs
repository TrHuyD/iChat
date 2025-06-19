using iChat.BackEnd.Services.Users.ChatServers.Abstractions;
using iChat.Data.EF;
using Microsoft.EntityFrameworkCore;
using System;

namespace iChat.BackEnd.Services.Users.Infra.EfCore.MessageServices
{
    public class EfCoreChatListingService : IChatListingService
    {
        private readonly iChatDbContext _dbContext;

        public EfCoreChatListingService(iChatDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<List<long>> GetServerChannelListAsync(long serverId)
        {
          
            return await _dbContext.ChatChannels
                .Where(c => c.ServerId == serverId)
                .Select(c => c.Id)
                .ToListAsync();
        }

        public async Task<List<long>> GetUserServersAsync(long userId)
        {
        
            return await _dbContext.UserChatServers
                .Where(ucs => ucs.UserId == userId)
                .Select(ucs => ucs.ChatServerId)
                .ToListAsync();
        }

        public async Task<List<long>> GetServerMembersAsync(long serverId)
        {
           
            return await _dbContext.UserChatServers
                .Where(ucs => ucs.ChatServerId == serverId)
                .Select(ucs => ucs.UserId)
                .ToListAsync();
        }
    }
}
