using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iChat.Data.Entities.Users
{
    public class Role : IdentityRole<long>
    {
        public string Description { get; set; }
    }
}
