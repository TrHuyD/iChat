using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

    namespace iChat.Data.Entities.Servers.ChatRoles
    {
        public enum Permission : ulong
        {
            None = 0,
            ViewChannel = 1UL << 0,
            SendMessages = 1UL << 1,
            ManageMessages = 1UL << 2,
        
        }
    }
