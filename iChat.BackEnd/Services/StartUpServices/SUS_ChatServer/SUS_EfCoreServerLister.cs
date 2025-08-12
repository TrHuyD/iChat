﻿using iChat.BackEnd.Models.ChatServer;
using iChat.BackEnd.Models.Helpers;
using iChat.Data.EF;
using iChat.DTOs.Users.Messages;
using Microsoft.EntityFrameworkCore;

namespace iChat.BackEnd.Services.StartUpServices.SUS_ChatServer
{
    public class SUS_EfCoreServerLister
    {
        private readonly iChatDbContext _db;

        public SUS_EfCoreServerLister(iChatDbContext db)
        {
            _db = db;
        }

        public async Task<List<ChatServerbulk>> GetAllServersWithChannelsAsync()
        {
            var servers = await _db.ChatServers
                .AsNoTracking()
                .Include(s => s.ChatChannels)
                .Include(s=>s.Emojis)
                .Select(s => new ChatServerbulk
                {
                    Id = s.Id,
                    Name = s.Name,
                    AvatarUrl = s.Avatar ?? "https://cdn.discordapp.com/embed/avatars/0.png",
                    CreatedAt=s.CreatedAt
                    ,
                    AdminId=s.AdminId,
                    Channels = s.ChatChannels
                        .OrderBy(c => c.Order)
                        .Select(c => new ChatChannelDtoLite
                        {
                            Id = c.Id.ToString(),
                            Name = c.Name,
                            Order = c.Order,
                            last_bucket_id = c.LastAssignedBucketId
                        })
                        .ToList(),
                    memberList= s.UserChatServers.Select(us =>us.UserId).ToList(),
                    Emojis=s.Emojis.Select(e=>e.ToBaseDto()).ToList()
                })
                .ToListAsync();

            return servers;
        }
    }
}
