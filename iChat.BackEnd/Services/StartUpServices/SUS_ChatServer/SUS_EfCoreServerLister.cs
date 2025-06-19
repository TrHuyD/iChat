using iChat.Data.EF;
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

        public async Task<Dictionary<long, List<long>>> GetAllServerChannelsAsync()
        {
            var flatData = await _db.ChatChannels
                .AsNoTracking()
                .Select(c => new { c.ServerId, c.Id })
                .ToListAsync();

            return flatData
                .GroupBy(x => x.ServerId)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(x => x.Id).ToList()
                );
        }
    }
}
