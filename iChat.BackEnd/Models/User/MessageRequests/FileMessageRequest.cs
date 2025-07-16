using iChat.ViewModels.Users.Messages;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace iChat.BackEnd.Models.User.MessageRequests
{
    public class FileMessageRequest
    {
 
            [FromForm(Name = "file")]
            [AllowedMediaTypes]    
            public required IFormFile File { get; set; }
            [FromForm(Name = "serverId")]
            [RegularExpression(@"^\d+$", ErrorMessage = "ServerId must be numeric.")]
            public required string ServerId { get; set; }

            [FromForm(Name = "channelId")]
            [RegularExpression(@"^\d+$", ErrorMessage = "ChannelId must be numeric.")]
            public required string ChannelId { get; set; }

    }
}
