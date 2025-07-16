using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iChat.DTOs.Users
{
    public class UserMetadata
    {
        public string UserId { get; set; } 
        public string DisplayName { get; set; }
        public string AvatarUrl { get; set; }
        public string Version { get; set; }  // Versioning 
        public UserMetadata(string UserId, string DisplayName, string AvatarUrl)
        {
            this.UserId = UserId;
            this.DisplayName = DisplayName;
            this.AvatarUrl = AvatarUrl;
            this.Version= DateTimeOffset.Now.ToUnixTimeSeconds().ToString();
        }
        public UserMetadata()
        {

        }
    }
}
