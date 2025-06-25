using iChat.Data.EF;
using iChat.DTOs.Users.Messages;
using Microsoft.EntityFrameworkCore;

namespace iChat.BackEnd.Services.StartUpServices.SUS_ChatServer
{
    public class SUS_EfCoreServerLister
    {
        private readonly iChatDbContext _db;

        public SUS_EfCoreServerLister(iChatDbContext db)
        {
            _db = db;
        }

        public async Task<List<ChatServerMetadata>> GetAllServersWithChannelsAsync()
        {
            var servers = await _db.ChatServers
                .AsNoTracking()
                .Include(s => s.ChatChannels)
                .Select(s => new ChatServerMetadata
                {
                    Id = s.Id.ToString(),
                    Name = s.Name,
                    AvatarUrl = s.Avatar ?? "https://cdn.discordapp.com/embed/avatars/0.png",
                    CreatedAt=s.CreatedAt
                    ,
                    Channels = s.ChatChannels
                        .OrderBy(c => c.Order)
                        .Select(c => new ChatChannelMetadata
                        {
                            Id = c.Id.ToString(),
                            Name = c.Name,
                            Order = c.Order,
                            last_bucket_id = c.LastAssignedBucketId
                        })
                        .ToList()
                })
                .ToListAsync();

            return servers;
        }
    }
}
