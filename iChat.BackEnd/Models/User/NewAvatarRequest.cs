﻿using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iChat.BackEnd.Models.User
{
    public class NewAvatarRequest
    {
        [FromForm(Name ="file")]
        public IFormFile File { get; set; }
    }
}
