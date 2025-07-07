using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iChat.DTOs.Users.Messages
{
    using System.ComponentModel.DataAnnotations;

    public class LastSeenUpdateRequest
    {
        [Required(ErrorMessage = "ServerId is required")]
        [RegularExpression(@"\S+", ErrorMessage = "ServerId cannot be empty or whitespace")]
        public string ServerId { get; set; }
        [Required(ErrorMessage = "ChatChannelId is required")]
        [RegularExpression(@"\S+", ErrorMessage = "ChatChannelId cannot be empty or whitespace")]
        public string ChatChannelId { get; set; }
        [Required(ErrorMessage = "MessageId is required")]
        [RegularExpression(@"\S+", ErrorMessage = "MessageId cannot be empty or whitespace")]
        public string MessageId { get; set; }
    }

}
