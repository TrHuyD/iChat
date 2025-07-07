using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iChat.DTOs.Users.Messages
{
    public class ChatChannelCreateRq
    {

        [Required(ErrorMessage = "Missing Server Id")]
        public string ServerId { get; set; }
        [Required(ErrorMessage ="Missing Channel name")]
        public string Name { get; set; }
        
    }
}
