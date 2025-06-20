using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iChat.Data.Entities.Users.Auth
{
    public class RefreshToken
    {
        public int Id { get; set; }
        public string Token { get; set; }
        public DateTimeOffset ExpiryDate { get; set; }
        public DateTimeOffset Created { get; set; }
        public DateTimeOffset? Revoked { get; set; }
        public bool IsExpired => DateTimeOffset.UtcNow >= ExpiryDate;
        public bool IsActive => Revoked == null && !IsExpired;
        public long UserId { get; set; }
        public AppUser User { get; set; }
        public string IpAddress { get; set; } // IP address of the user when the token was created
    }
}
