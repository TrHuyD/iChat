using iChat.Data.Entities.Users.Auth;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iChat.Data.Entities.Users
{
    public class AppUser : IdentityUser<long>
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime Dob { get; set; }
      
        public List<RefreshToken> RefreshTokens { get; set; } = new();
        public DateTime LastSeen { get; set; }

    }
}
