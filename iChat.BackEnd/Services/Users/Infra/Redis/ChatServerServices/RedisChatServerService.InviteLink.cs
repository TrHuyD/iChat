﻿using iChat.BackEnd.Services.Users.ChatServers.Abstractions;
using iChat.BackEnd.Services.Users.Infra.IdGenerator;
using iChat.BackEnd.Services.Users.Infra.Redis.Enums;
using iChat.DTOs.Collections;
using iChat.DTOs.Shared;

namespace iChat.BackEnd.Services.Users.Infra.Redis.ChatServerServices
{
    public partial class RedisCSInviteLinkService
    {
        private readonly AppRedisService _service;
        private readonly IChatServerMetadataCacheService _localCache;
        public SnowflakeService idGen { get; set; }
        public RedisCSInviteLinkService(AppRedisService redisService, IChatServerMetadataCacheService localCache,SnowflakeService snowflake)
        {
            idGen = snowflake;
            _service = redisService;
            _localCache = localCache;
        }
        private TimeSpan default_invite_lifetime = TimeSpan.FromDays(7); // Default invite link lifetime
        public async Task<OperationResultString> CreateInviteLink(stringlong serverId, stringlong userId)
        {
           var isAdmin = _localCache.IsAdmin(serverId,userId);
          
            if(!isAdmin.Success)
              return OperationResultString.Fail("400","Fail to create link, due "+isAdmin.ErrorMessage);
            if(isAdmin.Value)
                return OperationResultString.Fail("400", "Fail to create link because user is not Admin");
            var db = _service.GetDatabase();
            var serverkey = RedisVariableKey.GetServerInviteKey(serverId.ToString());
            var existimte = await db.KeyTimeToLiveAsync(serverkey);
            string inviteId = string.Empty;
            if (existimte!=null&&existimte.Value.Days>1)
            {
                inviteId = await db.StringGetAsync(serverkey);
                await db.KeyExpireAsync(serverkey, default_invite_lifetime);
               
            }
            else
                for (int i = 0; i < 10; i++)
                {

                    // Generate a unique invite ID
                    inviteId = idGen.GenerateId().Id.ToString();
                    if (await db.KeyExistsAsync(RedisVariableKey.GetInviteLinkKey(inviteId)))
                        break;
                    
                  

                }
            if (string.IsNullOrEmpty(inviteId))
            throw new InvalidOperationException("Failed to generate a unique invite link after multiple attempts.");
            await Task.WhenAll(db.StringSetAsync(RedisVariableKey.GetInviteLinkKey(inviteId), serverId.ToString(), default_invite_lifetime),
             db.StringSetAsync(serverkey, inviteId, default_invite_lifetime));
            return OperationResultString.Ok(inviteId);
        }
        public async Task<string> ParseInviteLink(string inviteId)
        {
            var db = _service.GetDatabase();
            var serverId = await db.StringGetAsync(RedisVariableKey.GetInviteLinkKey(inviteId));
            if (serverId.IsNullOrEmpty)
                throw new InvalidOperationException($"Invite link {inviteId} does not exist or has expired.");
            return serverId;
        }
    }
}
