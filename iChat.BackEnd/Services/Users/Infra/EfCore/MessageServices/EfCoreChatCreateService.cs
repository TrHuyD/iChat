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
        public async Task<ChatChannelDto> CreateChannelAsync(long serverId, string channelName, long adminUserId)
        {
            var result = await _db.ChatServers
                .Where(s => s.Id == serverId)
                .Select(s => new
                {
                    IsAdmin = s.AdminId == adminUserId,
                    ChannelCount = s.ChatChannels.Count
                })
                .FirstOrDefaultAsync();
            if (result == null)
                throw new InvalidOperationException($"Server {serverId} not found");
            if(result.IsAdmin== false)
                throw new UnauthorizedAccessException($"Not {adminUserId} have enough perm to create chat channel in server {serverId}");

            var channelId = _channelIdGen.GenerateId().Id;
            var channel = new ChatChannel
            {
                Id = channelId,
                Name = channelName,
                CreatedAt = DateTimeOffset.UtcNow,
                ServerId = serverId,
                Order= (short)result.ChannelCount, 
            };
            AssignBucket(channel);
            _db.ChatChannels.Add(channel);
            await _db.SaveChangesAsync();
           
            return new ChatChannelDto
            {
                Id = channelId.ToString(),
                Name = channelName,
                Order = channel.Order,
                last_bucket_id = 0 ,
                ServerId = serverId.ToString(),
            };
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
                AdminId = adminUserId,
                Avatar= "https://cdn.discordapp.com/embed/avatars/0.png",
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
                Channels = new List<ChatChannelDtoLite>
                {
                    new ChatChannelDtoLite
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
