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
        private const int expiryTime = 86400;
        private static readonly TimeSpan expiryTimeSpan = TimeSpan.FromSeconds(expiryTime);

        public RedisUserServerService(AppRedisService redisService)
        {
            _service = redisService;
        }

        public async Task<int> CheckIfUserInServer(long userId, long serverId)
        {
            var key = RedisVariableKey.GetUserServerKey(userId);
            return await _service.CheckAndExtendMembershipExpiryAsync(key, serverId, expiryTimeSpan);
        }

        public async Task<int> AddUserServersAsync(long userId, IEnumerable<ChatServerDto> servers)
        {
            var key = RedisVariableKey.GetUserServerKey(userId);
            await _service.PushToListAsync(key, servers.ToList(), expiryTimeSpan);
            return servers.Count();
        }

        public async Task<List<ChatServerDto>?> GetUserServersAsync(long userId)
        {
            var key = RedisVariableKey.GetUserServerKey(userId);
            return await _service.GetListAsync<ChatServerDto>(key);
        }

        public async Task<List<string>?> GetServerChannelsAsync(long serverId)
        {
            var key = RedisVariableKey.GetServerChannelKey(serverId);
            return await _service.GetListAsync<string>(key);
        }

        public async Task<int> AddServerChannelsAsync(long serverId, IEnumerable<string> channelIds)
        {
            var key = RedisVariableKey.GetServerChannelKey(serverId);
            await _service.PushToListAsync(key, channelIds.ToList(), expiryTimeSpan);
            return channelIds.Count();
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
                throw new InvalidOperationException("Redis function execution failed", ex);
            }
        }
    }
}
