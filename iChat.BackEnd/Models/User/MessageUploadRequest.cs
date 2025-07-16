using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace iChat.BackEnd.Models.User
{
    public class MessageUploadRequest
    {
        [FromForm(Name = "file")]
        public IFormFile File { get; set; }

        [FromForm(Name = "channelId")]
        [RegularExpression(@"^\d+$", ErrorMessage = "ChannelId must be numeric.")]
        public string ChannelId { get; set; }
        [FromForm(Name ="serverId")]
        [RegularExpression(@"^\d+$", ErrorMessage = "ServerId must be numeric.")]
        public string ServerId { get; set; }

        //[FromForm(Name = "messageType")]
        //public string MessageType { get; set; }

        //[FromForm(Name = "textContent")]
        //public string? TextContent { get; set; }
    }
}
