using iChat.BackEnd.Services.Users.ChatServers.Abstractions;
using iChat.Data.EF;
using iChat.Data.Entities.Servers;
using Microsoft.EntityFrameworkCore;
using System;

namespace iChat.BackEnd.Services.Users.Infra.EfCore.MessageServices
{
    public class EfCoreChatServerEditService : IChatServerEditService
    {
        private readonly iChatDbContext _db;

        public EfCoreChatServerEditService(iChatDbContext db)
        {
            _db = db;
        }

        public async Task<bool> UpdateChatServerNameAsync(string serverId, string newName, string adminUserId)
        {
            if (!long.TryParse(serverId, out var sid) || !long.TryParse(adminUserId, out var uid))
                return false;

            var server = await _db.ChatServers
                .FirstOrDefaultAsync(cs => cs.Id == sid && cs.AdminId == uid);





            if (server == null)
                return false;

            server.Name = newName;
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteChatServerAsync(string serverId, string adminUserId)
        {
            if (!long.TryParse(serverId, out var sid) || !long.TryParse(adminUserId, out var uid))
                return false;

            var server = await _db.ChatServers
                .FirstOrDefaultAsync(cs => cs.Id == sid && cs.AdminId == uid);

            if (server == null)
                return false;

            _db.ChatServers.Remove(server);
            await _db.SaveChangesAsync();
            return true;
        }
    }
}
