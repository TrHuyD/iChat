﻿using iChat.DTOs.Users.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iChat.DTOs.Users
{
    public class UserCompleteDto
    {
     public   List<ChatServerDtoUser> ChatServers { get; set; } 
     public UserProfileDto UserProfile { get; set; }
     public List<string> FriendList { get; set; } = new List<string>();
        public List<string> BlockedList { get; set; } = new List<string>();
    }
}
