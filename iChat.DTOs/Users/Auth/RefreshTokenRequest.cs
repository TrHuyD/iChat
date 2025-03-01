using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iChat.DTOs.Users.Auth
{
    public class RefreshTokenRequest
    {
        public string Jwt { get; set; }
        public string RefreshToken { get; set; }
    }

}
