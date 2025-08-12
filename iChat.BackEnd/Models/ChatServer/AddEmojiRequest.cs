using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iChat.DTOs.Users.Servers
{
    public class AddEmojiRequest
    {
        [FromForm(Name ="file")]
        public IFormFile file { get; set; }
    }
}
