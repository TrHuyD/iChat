using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iChat.DTOs.Users.Auth
{
    public class TokenResponse
    {
        public string AccessToken { get; set; }
        public DateTime ExpireTime { get; set; }
        public TokenResponse(string accessToken, DateTime expireTime)
        {
            AccessToken = accessToken;
            ExpireTime = expireTime;
        }
    }
}
