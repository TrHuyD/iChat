using iChat.BackEnd.Models.Infrastructures;
using iChat.BackEnd.Services.Users.ChatServers.Abstractions.DB;
using iChat.Data.EF;
using iChat.Data.Entities.Servers;
using iChat.DTOs.Collections;
using iChat.DTOs.Users.Messages;
using Microsoft.EntityFrameworkCore;
using Polly;

namespace iChat.BackEnd.Services.Users.Infra.EfCore.MessageServices
{
    public class EfCoreChatServerService : IChatServerDbService
    {
        private readonly iChatDbContext _db;

        public EfCoreChatServerService(iChatDbContext db)
        {
            _db = db;
        }

        public Task<bool> CheckIfUserInServer(stringlong userId, stringlong serverId)
        {
            return _db.UserChatServers
                .AnyAsync(us => us.UserId == userId && us.ChatServerId == serverId);
        }

        public Task<bool> CheckIfUserBanned(stringlong userId, stringlong serverId)
        {
            return _db.ServerBans
                .AnyAsync(bu => bu.UserId == userId && bu.ChatServerId == serverId);
        }

        public async Task BanUserAsync(stringlong userId, stringlong serverId, stringlong adminUserId)
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
        public async Task UnbanUser(stringlong userId, stringlong serverId, stringlong adminUserId)
        {
            var ban = await _db.ServerBans
                .FirstOrDefaultAsync(b => b.UserId == userId && b.ChatServerId == serverId);
            if (ban == null)
                return; 
            _db.ServerBans.Remove(ban);
            await _db.SaveChangeAsyncSafe();
        }
        public async Task Join(stringlong userId, stringlong serverId)
        {
            var isBanned = await CheckIfUserBanned(userId, serverId);
            if (isBanned)
                throw new Exception($"User {userId} is banned from server {serverId}.");

            var alreadyMember = await CheckIfUserInServer(userId, serverId);
            if (alreadyMember)
                return; 
            var channelsWithLastMessage = await _db.ChatChannels
                .Where(c => c.ServerId == serverId)
                .Select(c => new
                {
                    ChannelId = c.Id,
                    LastMessageId = _db.Messages
                        .Where(m => m.ChannelId == c.Id)
                        .OrderByDescending(m => m.BucketId)
                        .ThenByDescending(m => m.Id)
                        .Select(m => m.Id)
                        .FirstOrDefault()
                })
                .ToListAsync();
                var userChatChannels = channelsWithLastMessage
                    .Select(x => new UserChatChannel
                    {
                        UserId = userId,
                        ChannelId = x.ChannelId,
                        LastSeenMessage = x.LastMessageId, 
                        NotificationCount = 0
                    })
                    .ToList();
            _db.UserChatChannels.AddRange(userChatChannels);
            await _db.SaveChangeAsyncSafe();
        }

        public async Task Left(stringlong userId, stringlong serverId)
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

        private async Task EditServer(stringlong serverId,string newName="", string url="")
        {
            if (newName == "" && url == "")
                throw new Exception("Method being used wrong");
            var server = await _db.ChatServers.FirstOrDefaultAsync(cs => cs.Id == serverId );
            if (server == null)
                throw new UnauthorizedAccessException("You do not have permission to update this server.");
            if(!(newName==""))
                server.Name = newName;
            if (!(url == ""))
                server.Avatar = url;
            await _db.SaveChangesAsync();
            return;


        }
        public async Task<ChatServerChangeUpdate> UpdateChatServerProfileAsync(stringlong serverId, stringlong adminUserId, string newName="", string url="")
        {
            try
            {
                await EditServer(serverId, newName,url);
                return new ChatServerChangeUpdate
                {
                    Id = serverId.ToString(),
                    Name = newName,
                    AvatarUrl = url
                };

            }
            catch (Exception ex)
            {
                throw new UnauthorizedAccessException($"An error has occured {ex.Message}");

            }
            }

        public async Task TaskDeleteChatServerAsync(stringlong serverId, stringlong adminUserId)
        {
            var server = await _db.ChatServers
                .FirstOrDefaultAsync(cs => cs.Id == serverId);

            if (server == null || server.AdminId != adminUserId)
                throw new UnauthorizedAccessException("You do not have permission to delete this server.");

            _db.ChatServers.Remove(server);
            await _db.SaveChangesAsync();
        }
    }
}
