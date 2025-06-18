using iChat.Data.Entities.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iChat.Data.Entities.Servers.ChatRoles
{
    public class UserChatRole
    {
        public long UserId { get; set; }
        public AppUser User { get; set; }

        public long RoleId { get; set; }
        public ChatRole Role { get; set; }

       
    }
}
