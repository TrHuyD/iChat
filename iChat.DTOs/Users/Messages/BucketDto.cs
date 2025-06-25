﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace iChat.DTOs.Users.Messages
{
    public class BucketDto
    {
        public int BucketId { get; set; }
        public string ChannelId { get; set; }   
        public int MessageCount { get; set; }
        public string FirstSequence { get; set; }
        public string LastSequence { get; set; }
        public List<ChatMessageDtoSafe> ChatMessageDtos { get; set; } = new List<ChatMessageDtoSafe>();
        public BucketDto(RawBucketResult r)
        {
            BucketId = r.bucket_id;
            ChannelId = r.channel_id.ToString();
            MessageCount = (int)r.message_count;
            var s= JsonSerializer.Deserialize<List<ChatMessageDto>>(r.messages);
            ChatMessageDtos = s.Select(t => new ChatMessageDtoSafe(t)).ToList();
            ChatMessageDtos.Sort((a, b) => a.Id.CompareTo(b.Id));
            if(ChatMessageDtos.Count == 0)
            {
                FirstSequence = "";
                LastSequence = "";
                return;
            }
            FirstSequence = ChatMessageDtos[0].Id ;
            LastSequence = ChatMessageDtos[^1].Id;

        }
    }
}
