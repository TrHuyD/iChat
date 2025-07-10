using iChat.BackEnd.Models.Infrastructures;
using iChat.BackEnd.Services.Users.ChatServers.Abstractions;
using iChat.Data.EF;
using iChat.Data.Entities.Servers;
using iChat.DTOs.Shared;
using Microsoft.EntityFrameworkCore;

namespace iChat.BackEnd.Services.Users.Infra.EfCore.MessageServices
{
    public class EfCoreChatServerService:IChatServerService
    {
        private readonly iChatDbContext _db;
        public EfCoreChatServerService(iChatDbContext db)
        {
                       _db = db;
        }
        public async Task<bool> CheckIfUserInServer(long userId, long serverId)
        {
            return await _db.UserChatServers.AnyAsync(us => us.UserId == userId && us.ChatServerId == serverId);
        }
        public async Task<bool> CheckIfUserBanned(long userId, long serverId)
        {
            return await _db.ServerBans.AnyAsync(bu => bu.UserId == userId && bu.ChatServerId == serverId);
        }
        private async Task<MembershipCheckDto> CheckMembershipStatusAsync(long userId, long serverId)
        {
            var sql = @"
                WITH vars AS (
                    SELECT {0}::BIGINT AS user_id, {1}::BIGINT AS server_id
                )
                SELECT
                    EXISTS (
                        SELECT 1 FROM ""UserChatServers"" ucs
                        JOIN vars v ON true
                        WHERE ucs.""UserId"" = v.user_id AND ucs.""ChatServerId"" = v.server_id
                    ) AS is_member,
                    EXISTS (
                        SELECT 1 FROM ""ServerBans"" sb
                        JOIN vars v ON true
                        WHERE sb.""UserId"" = v.user_id AND sb.""ChatServerId"" = v.server_id
                    ) AS is_banned;
            ";
            return await _db.Set<MembershipCheckDto>()
                .FromSqlRaw(sql, userId, serverId)
                .AsNoTracking()
                .FirstAsync();
        }

        public async Task BanUser(long userId, long serverId, long adminUserId)
        {

            var result = await CheckMembershipStatusAsync(userId, serverId);
            if (result.Is_Member == false)
                throw new Exception($"User {userId} is not a member of the server.");
            if (result.Is_Banned)
                throw new Exception($"User {userId} is already banned from the server.");
            var ban = new ServerBan
            {
                UserId = userId,
                ChatServerId = serverId,
                BannedById = adminUserId,
                BannedAt = DateTimeOffset.UtcNow
            };
            _db.ServerBans.Add(ban);
            await _db.SaveChangeAsyncSafe();
        }
        public async Task UnbanUser(long userId, long serverId, long adminUserId)
        {
            var result = await CheckMembershipStatusAsync(userId, serverId);
            if (result.Is_Member == false)
                throw new Exception($"User {userId} is not a member of the server.");
            if (result.Is_Banned == false)
                throw new Exception($"User {userId} is not banned from the server.");
            var ban = await _db.ServerBans
                .FirstOrDefaultAsync(b => b.UserId == userId && b.ChatServerId == serverId);
            if (ban != null)
            {
                
                _db.ServerBans.Remove(ban);
                await _db.SaveChangeAsyncSafe();
            }
        }
        public async Task Join(long userId, long serverId)
        {

            if (await CheckIfUserBanned(userId,serverId))
                throw new Exception($"User {userId} is banned from the server.");
            var userServer = new UserChatServer
            {
                UserId = userId,
                ChatServerId = serverId,
                JoinedAt = DateTimeOffset.UtcNow
            };
            _db.UserChatServers.Add(userServer);
            await _db.SaveChangeAsyncSafe();
        }
        private class MembershipCheckDto
        {
            public bool Is_Member { get; set; }
            public bool Is_Banned { get; set; }
        }

    }
}
