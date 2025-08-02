using iChat.DTOs.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iChat.DTOs.Users
{
    public class UserOnlineStatusChangedEventArgs
    {
        public UserId userId { get; init; }
        public bool IsOnline { get; init; }
    }
}
