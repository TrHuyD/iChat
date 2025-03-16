using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iChat.DTOs.Users.Auth
{
    public class LoginRequest
    {
       
        public string Username { get; set; }    

        public string Password { get; set; }

        public bool RememberMe { set; get; }
    }
}
