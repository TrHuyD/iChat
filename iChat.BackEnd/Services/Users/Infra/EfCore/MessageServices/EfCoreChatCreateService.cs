using iChat.BackEnd.Services.Users.ChatServers.Abstractions;
using iChat.BackEnd.Services.Users.Infra.IdGenerator;
using iChat.Data.EF;
using iChat.Data.Entities.Servers;
using iChat.Data.Entities.Users.Messages;
using iChat.DTOs.Users.Messages;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Channels;

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
        private void AssignBucket(ChatChannel channel)
        {
            channel.Buckets.Add(new Bucket
            {
                BucketId = int.MaxValue,
                CreatedAt = DateTimeOffset.UtcNow,
                ChannelId = channel.Id
            });
            channel.Buckets.Add(new Bucket
            {
                BucketId = 0,
                CreatedAt = DateTimeOffset.UtcNow,
                ChannelId = channel.Id
            });
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
            AssignBucket(channel);
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

        public async Task<ChatServerDto> CreateServerAsync(string serverName, long adminUserId)
        {
            var serverId = _serverIdGen.GenerateId();
            var generalChannelId = _channelIdGen.GenerateId();

            var server = new ChatServer
            {
                Id = serverId.Id,
                Name = serverName,
                CreatedAt = serverId.CreatedAt,
                ChatChannels = new List<ChatChannel>(),
                AdminId = adminUserId
            };

            var generalChannel = new ChatChannel
            {
                Id = generalChannelId.Id,
                Name = "general",
                CreatedAt = generalChannelId.CreatedAt,
                Server = server
            };
            AssignBucket(generalChannel);
            server.ChatChannels.Add(generalChannel);

            _db.UserChatServers.Add(new UserChatServer
            {
                UserId = adminUserId,
                ChatServerId = serverId.Id
            });



            _db.ChatServers.Add(server);
            await _db.SaveChangesAsync();

            return new ChatServerDto
            {
                AvatarUrl = "https://cdn.discordapp.com/embed/avatars/0.png",
                Id = serverId.Id.ToString(),
                Name = serverName,
             //   CreatedAt = serverId.CreatedAt,
                Channels = new List<ChatChannelMetadata>
                {
                    new ChatChannelMetadata
                    {
                        Id = generalChannelId.Id.ToString(),
                        Name = "general",
                        Order = 0,
                        last_bucket_id = 0
                    }
                }

            };
        }
    }
}
