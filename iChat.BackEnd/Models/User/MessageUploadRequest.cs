using Microsoft.AspNetCore.Mvc;

namespace iChat.BackEnd.Models.User
{
    public class MessageUploadRequest
    {
        [FromForm(Name = "file")]
        public IFormFile File { get; set; }

        [FromForm(Name = "channelId")]
        public string ChannelId { get; set; }
        [FromForm(Name ="serverId")]
        public string ServerId { get; set; }

        //[FromForm(Name = "messageType")]
        //public string MessageType { get; set; }

        //[FromForm(Name = "textContent")]
        //public string? TextContent { get; set; }
    }
}
