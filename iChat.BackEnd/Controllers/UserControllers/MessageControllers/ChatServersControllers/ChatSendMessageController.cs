﻿using iChat.BackEnd.Models.User.MessageRequests;
using iChat.BackEnd.Services.Users.ChatServers;
using iChat.BackEnd.Services.Users.ChatServers.Abstractions;
using iChat.DTOs.Users.Messages;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace iChat.BackEnd.Controllers.UserControllers.MessageControllers.ChatServersControllers
{
    [ApiController]
    [Route("api/chat")]
    [Authorize]
    public class ChatSendMessageController :ControllerBase
    {
        readonly IMessageWriteService _writeService;
        readonly ChatHubResponer _chatHub;
        public ChatSendMessageController(IMessageWriteService writeService, ChatHubResponer chatHub)
        {
            _writeService = writeService;
            _chatHub = chatHub;
        }
        [HttpPost("EditMessage")]
        public async Task<IActionResult> EditMessage([FromBody] UserEditMessageRq rq)
        {
            if (rq == null ||!ModelState.IsValid)
            {
                return BadRequest($"Invalid request data.");
            }
            string userId = new UserClaimHelper(User).GetUserIdStr();
            try
            {
                await _chatHub.EditedMessage(await _writeService.EditMessageAsync(rq, userId));
            }
            catch(Exception ex)
            {
                return BadRequest($"Fail to edit message {ex.Message}");
            }

            return Ok("Message edited successfully.");
        }
        [HttpPost("DeleteMessage")]
        public async Task<IActionResult> DeleteMessage([FromBody] UserDeleteMessageRq rq)
        {
            if (rq == null || !ModelState.IsValid)
            {
                return BadRequest($"Invalid request data.");
            }
            string userId = new UserClaimHelper(User).GetUserIdStr();
            try
            {
               await _chatHub.DeletedMessage(await _writeService.DeleteMessageAsync(rq, userId));
            }
            catch (Exception ex)
            {   
                return BadRequest($"Fail to delete message {ex.Message}");
            }
                return Ok("Message deleted successfully.");
        }
        //[HttpPost("api/upload")]
        //[RequestSizeLimit(6 * 1024 * 1024)]

        //[HttpPost("{channelId}/send")]
        //public async Task<IActionResult> SendMessage(string channelId, [FromBody] UserWeb_MessageRequest request)
        //{
        //    if (!long.TryParse(channelId, out long longChannelId))
        //    {
        //        return BadRequest("Valid channel ID is required.");
        //    }
        //    if (request == null)
        //    {
        //        return BadRequest("Message request is required.");
        //    }
        //    string userId = new UserClaimHelper(User).GetUserIdStr();
        //    var r = new MessageRequest
        //    {
        //        SenderId= userId,
        //        ReceiveChannelId = channelId,
        //        TextContent = request.TextContent,
        //        MediaContent = request.MediaContent,
        //    };
        //    var result = await _writeService.SendTextMessageAsync(r);
        //    if (result.Success)
        //    {
        //        return Ok("Message sent successfully.");
        //    }
        //    else
        //    {
        //        return StatusCode(500, "Failed to send message.");
        //    }
        //}
        //[HttpPost]
        //[HttpPost("{channelId}/send")]
        //public async Task<IActionResult> SendMessageApi()
    }
}
