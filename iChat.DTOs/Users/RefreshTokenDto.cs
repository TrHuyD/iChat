using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iChat.DTOs.Users
{
    public class RefreshTokenDto
    {
        public string Token { get; set; }
        public DateTime ExpiryDate { get; set; }
    }
}
