using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iChat.DTOs.Users.Messages
{
    public class UserEditMessageRq
    {

        [Required]
      public  string ChannelId { get; set; }
        [Required]
      public  string MessageId { get; set; }
        [Required]
        [MinLength(1, ErrorMessage = "New content must be at least 1 character long.")]
        public string NewContent { get; set; }

    }
}
