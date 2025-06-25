using iChat.BackEnd.Services.Users.ChatServers.Abstractions;
using iChat.Data.EF;
using iChat.DTOs.Users.Messages;
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
        public async Task<List<ChatServerDto>> GetUserChatServersAsync(long userId)
        {
            return await _dbContext.UserChatServers
                .Where(ucs => ucs.UserId == userId)
                .Select(ucs => new ChatServerDto
                {
                    Id = ucs.ChatServer.Id.ToString(),
                    Name = ucs.ChatServer.Name,
                    AvatarUrl = ucs.ChatServer.Avatar ?? "https://cdn.discordapp.com/embed/avatars/0.png",
                    Position = ucs.Order,
                    Channels = ucs.ChatServer.ChatChannels
                     //   .OrderBy(c => c.Order) // Optional: sort channels by order
                        .Select(c => new ChatChannelMetadata
                        {
                            Id = c.Id.ToString(),
                            Name = c.Name,
                            Order = c.Order
                        })
                        .ToList()
                })
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
