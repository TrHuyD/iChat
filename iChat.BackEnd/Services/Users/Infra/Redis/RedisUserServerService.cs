using iChat.BackEnd.Services.Users.Infra.Redis.Enums;
using iChat.DTOs.Users.Messages;
using iChat.ViewModels.Users;
using Microsoft.AspNetCore.Mvc;
using StackExchange.Redis;
using System.Text.Json;

namespace iChat.BackEnd.Services.Users.Infra.Redis
{
    public class RedisUserServerService
    {
        private readonly AppRedisService _service;
        int expiryTime = 86400;
        TimeSpan expiryTimeSpan = TimeSpan.FromSeconds(86400);
        public RedisUserServerService(AppRedisService redisService)
        {
            _service = redisService;
        }
        

        public async Task<int> CheckIfUserInServer(long userId, long serverId)
        {
            var key = RedisVariableKey.GetUserServerKey(userId);
            return await _service.CheckAndExtendMembershipExpiryAsync(key, serverId, expiryTimeSpan);
        }
        public async Task<int> AddUserServersAsync(long userId, IEnumerable<string> serverIds)
        {
            var key = RedisVariableKey.GetUserServerKey(userId);
            var result = await _service.AddListAsync(key, expiryTime, serverIds);
            return (int)result!;
        }
        public async Task<List<string>?> GetUserServersAsync(long userId)
        {
            var serverKey = RedisVariableKey.GetUserServerKey(userId);
            var result = await _service.GetListAsync(serverKey);
            return result;
        }
        public async Task<List<string>?> GetServerChannelsAsync(long serverId)
        {
            var key = RedisVariableKey.GetServerChannelKey(serverId);
            return await _service.GetListAsync(key);
        }
        public async Task<int> AddServerChannelsAsync(long serverId, IEnumerable<string> channelIds)
        {
            var key = RedisVariableKey.GetServerChannelKey(serverId);
            int expiryTime = 86400;
            var result = await _service.AddListAsync(key, expiryTime, channelIds);
            return (int)result!;
        }


        public async Task<ChannelPermissionResult> CheckUserChannelPermissionAsync(long userId, long serverId, long channelId)
        {
            var keys = new RedisKey[]
            {
                userId.ToString(),
                serverId.ToString(),
                channelId.ToString()
            };

            try
            {
                var result = await _service.GetDatabase().ExecuteAsync("FCALL", "check_channel_perm", 3, keys);
                int intValue = (int)result;

                return Enum.IsDefined(typeof(ChannelPermissionResult), intValue)
                    ? (ChannelPermissionResult)intValue
                    : ChannelPermissionResult.Unknown;
            }
            catch (RedisServerException ex)
            {
                // Optionally log or handle unexpected errors
                throw new InvalidOperationException("Redis function execution failed", ex);
            }
        }


    }
}
    