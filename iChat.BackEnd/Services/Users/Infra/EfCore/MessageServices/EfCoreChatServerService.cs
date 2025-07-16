using iChat.BackEnd.Models.Infrastructures;
using iChat.BackEnd.Services.Users.ChatServers.Abstractions.DB;
using iChat.Data.EF;
using iChat.Data.Entities.Servers;
using Microsoft.EntityFrameworkCore;

namespace iChat.BackEnd.Services.Users.Infra.EfCore.MessageServices
{
    public class EfCoreChatServerService : IChatServerDbService
    {
        private readonly iChatDbContext _db;

        public EfCoreChatServerService(iChatDbContext db)
        {
            _db = db;
        }

        public Task<bool> CheckIfUserInServer(long userId, long serverId)
        {
            return _db.UserChatServers
                .AnyAsync(us => us.UserId == userId && us.ChatServerId == serverId);
        }

        public Task<bool> CheckIfUserBanned(long userId, long serverId)
        {
            return _db.ServerBans
                .AnyAsync(bu => bu.UserId == userId && bu.ChatServerId == serverId);
        }

        public async Task BanUserAsync(long userId, long serverId, long adminUserId)
        {
            // Start a transaction
            using var transaction = await _db.Database.BeginTransactionAsync();

            try
            {
                var status = await _db.UserChatServers
                    .Where(ucs => ucs.UserId == userId && ucs.ChatServerId == serverId)
                    .Select(ucs => new
                    {
                        IsBanned = _db.ServerBans
                            .Any(sb => sb.UserId == userId && sb.ChatServerId == serverId)
                    })
                    .FirstOrDefaultAsync();
                if (status == null)
                    throw new InvalidOperationException($"User {userId} is not a member of server {serverId}.");
                if (status.IsBanned)
                    throw new InvalidOperationException($"User {userId} is already banned from server {serverId}.");
                // Remove all user memberships
                await _db.UserChatServers
                    .Where(ucs => ucs.UserId == userId && ucs.ChatServerId == serverId)
                    .ExecuteDeleteAsync();
                // Add ban record
                _db.ServerBans.Add(new ServerBan
                {
                    UserId = userId,
                    ChatServerId = serverId,
                    BannedById = adminUserId,
                    BannedAt = DateTime.UtcNow
                });

                await _db.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
        public async Task UnbanUser(long userId, long serverId, long adminUserId)
        {
            var ban = await _db.ServerBans
                .FirstOrDefaultAsync(b => b.UserId == userId && b.ChatServerId == serverId);
            if (ban == null)
                return; 
            _db.ServerBans.Remove(ban);
            await _db.SaveChangeAsyncSafe();
        }
        public async Task Join(long userId, long serverId)
        {
            var isBanned = await CheckIfUserBanned(userId, serverId);
            if (isBanned)
                throw new Exception($"User {userId} is banned from server {serverId}.");

            var alreadyMember = await CheckIfUserInServer(userId, serverId);
            if (alreadyMember)
                return; // Idempotent

            _db.UserChatServers.Add(new UserChatServer
            {
                UserId = userId,
                ChatServerId = serverId,
                JoinedAt = DateTime.UtcNow
            });

            await _db.SaveChangeAsyncSafe();
        }

        public async Task Left(long userId, long serverId)
        {
            // Idempotent delete
            var entry = await _db.UserChatServers
                .FirstOrDefaultAsync(ucs => ucs.UserId == userId && ucs.ChatServerId == serverId);

            if (entry != null)
            {
                _db.UserChatServers.Remove(entry);
                await _db.SaveChangeAsyncSafe();
            }
        }

        public async Task UpdateChatServerNameAsync(long serverId, string newName, long adminUserId)
        {
            var server = await _db.ChatServers
                .FirstOrDefaultAsync(cs => cs.Id == serverId && cs.AdminId == adminUserId);

            if (server == null)
                throw new UnauthorizedAccessException("You do not have permission to update this server.");

            server.Name = newName;
            await _db.SaveChangesAsync();
        }
        
        public async Task TaskDeleteChatServerAsync(long serverId, long adminUserId)
        {
            var server = await _db.ChatServers
                .FirstOrDefaultAsync(cs => cs.Id == serverId);
           
            if (server == null||server.AdminId!=adminUserId)
                throw new UnauthorizedAccessException("You do not have permission to delete this server.");

            _db.ChatServers.Remove(server);
            await _db.SaveChangesAsync();
        }

        public async Task UpdateChatServerProfileAsync(long serverId, string newName, string url, long adminUserId)
        {
            try
            {
                var server = await _db.ChatServers
                    .FirstOrDefaultAsync(cs => cs.Id == serverId);
                if (server == null)
                    throw new UnauthorizedAccessException("Server not found in server.");
                if (server.AdminId != adminUserId)
                    throw new UnauthorizedAccessException("You do not have permission to edit this server.");
                server.Avatar = url;
                server.Name = newName;
                await _db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new UnauthorizedAccessException($"An error has occured {ex.Message}");

            }
            }
    }
}
