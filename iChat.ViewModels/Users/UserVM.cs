﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iChat.ViewModels.Users
{
    public class UserVM
    {
        public long Id { get; set; }
        public string Username { get; set; }
        public string LastName { get; set; }
        public string FirstName { get; set; }
        
        public UserVM()
        {

        }
    }
}
