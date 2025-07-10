using iChat.BackEnd.Services.Users.ChatServers.Abstractions;
using iChat.BackEnd.Services.Users.Infra.Redis.Enums;
using iChat.DTOs.Users.Messages;
using StackExchange.Redis;
using System.Text.Json;

namespace iChat.BackEnd.Services.Users.Infra.Redis.ChatServerServices
{
    public partial class RedisChatServerService
    {
        private readonly AppRedisService _service;
        IChatServerMetadataCacheService _localCache;
        public RedisChatServerService(AppRedisService redisService, IChatServerMetadataCacheService localCache)
        {
            _service = redisService;
            _localCache = localCache;
        }


    

    }
}
