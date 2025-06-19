using iChat.BackEnd.Services.Users.ChatServers.Abstractions;
using iChat.BackEnd.Services.Users.Infra.IdGenerator;
using iChat.Data.EF;
using iChat.Data.Entities.Servers;
using Microsoft.EntityFrameworkCore;
using System;

namespace iChat.BackEnd.Services.Users.Infra.EfCore.MessageServices
{

    public class EfCoreChatCreateService : IChatCreateService
    {
        private readonly iChatDbContext _db;
        private readonly ChannelIdService _channelIdGen;
        private readonly ServerIdService _serverIdGen;

        public EfCoreChatCreateService(iChatDbContext db, ChannelIdService channelIdGen, ServerIdService serverIdGen)
        {
            _db = db;
            _channelIdGen = channelIdGen;
            _serverIdGen = serverIdGen;
        }

        public async Task<long> CreateChannelAsync(long serverId, string channelName, long adminUserId)
        {
            var channelId = _channelIdGen.GenerateId().Id;

            var server = await _db.ChatServers
                .Include(s => s.ChatChannels)
                .FirstOrDefaultAsync(s => s.Id == serverId);

            if (server == null)
                throw new InvalidOperationException("Server not found");

            var channel = new ChatChannel
            {
                Id = channelId,
                Name = channelName,
                CreatedAt = DateTimeOffset.UtcNow,
                ServerId = server.Id
            };

            _db.ChatChannels.Add(channel);

            var isMember = await _db.UserChatServers.AnyAsync(x => x.UserId == adminUserId && x.ChatServerId == serverId);
            if (!isMember)
            {
                _db.UserChatServers.Add(new UserChatServer
                {
                    UserId = adminUserId,
                    ChatServerId = serverId
                });
            }

            await _db.SaveChangesAsync();
            return channelId;
        }

        public async Task<string> CreateServerAsync(string serverName, long adminUserId)
        {
            var serverId = _serverIdGen.GenerateId().Id;
            var generalChannelId = _channelIdGen.GenerateId().Id;

            var server = new ChatServer
            {
                Id = serverId,
                Name = serverName,
                CreatedAt = DateTimeOffset.UtcNow,
                ChatChannels = new List<ChatChannel>()
            };

            var generalChannel = new ChatChannel
            {
                Id = generalChannelId,
                Name = "general",
                CreatedAt = DateTimeOffset.UtcNow,
                Server = server
            };

            server.ChatChannels.Add(generalChannel);

            _db.UserChatServers.Add(new UserChatServer
            {
                UserId = adminUserId,
                ChatServerId = serverId
            });



            _db.ChatServers.Add(server);
            await _db.SaveChangesAsync();

            return serverId.ToString();
        }
    }
}
