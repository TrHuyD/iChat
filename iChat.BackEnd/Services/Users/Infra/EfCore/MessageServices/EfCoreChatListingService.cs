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

        public async Task<List<long>> GetServerChannelListAsync(string serverId)
        {
            var sid = long.Parse(serverId);
            return await _dbContext.ChatChannels
                .Where(c => c.ServerId == sid)
                .Select(c => c.Id)
                .ToListAsync();
        }

        public async Task<List<long>> GetUserServersAsync(string userId)
        {
            var uid = long.Parse(userId);
            return await _dbContext.UserChatServers
                .Where(ucs => ucs.UserId == uid)
                .Select(ucs => ucs.ChatServerId)
                .ToListAsync();
        }

        public async Task<List<long>> GetServerMembersAsync(string serverId)
        {
            var sid = long.Parse(serverId);
            return await _dbContext.UserChatServers
                .Where(ucs => ucs.ChatServerId == sid)
                .Select(ucs => ucs.UserId)
                .ToListAsync();
        }
    }
}
