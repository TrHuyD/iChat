using iChat.DTOs.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iChat.DTOs.Users
{
    public class UserMetadata
    {
        public UserId userId { get; set; } 
        public string DisplayName { get; set; }
        public string AvatarUrl { get; set; }
        public string Version { get; set; }  // Versioning 
        public UserMetadata(UserId userId, string DisplayName, string AvatarUrl)
        {
            this.userId = userId;
            this.DisplayName = DisplayName;
            this.AvatarUrl = AvatarUrl;
            this.Version= DateTimeOffset.Now.ToUnixTimeSeconds().ToString();
        }
        public UserMetadata()
        {

        }
    }
}
