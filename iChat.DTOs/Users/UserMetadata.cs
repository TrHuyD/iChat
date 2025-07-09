using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iChat.DTOs.Users
{
    public record UserMetadata(string UserId, string DisplayName, string AvatarUrl);
}
