using iChat.BackEnd.Models.ChatServer;
using iChat.BackEnd.Models.User;
using iChat.BackEnd.Services.Users.ChatServers.Application;
using iChat.DTOs.Collections;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace iChat.BackEnd.Controllers.UserControllers.MessageControllers.ChatServersControllers
{
    [ApiController]
    [Authorize]
    [Route("api/Chat/Edit")]
    public class ChatServerEditController :ControllerBase
    {
        AppChatServerService _editor;
        public ChatServerEditController(AppChatServerService editor)
        {
            _editor=editor;
        }
        [HttpPost("Name")]
        public async Task<IActionResult> UpdateServerName([FromBody] NewNameServerRequest request)
        {

            if (ModelState.IsValid == false)
                return BadRequest(ModelState);
            try
            {
                var userId = new UserClaimHelper(User).GetUserIdSL();
                var result = await _editor.EditServerProfile(userId,new ServerId( request.ServerId), request.Name);
                if(result.Failure)
                    return BadRequest(result.ErrorMessage);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpPost("Avatar")]
        [RequestSizeLimit(6 * 1024 * 1024)]
        public async Task<IActionResult> UpdateAvatar([FromForm] NewChatServerAvatarRequest request)
        {
            try
            {
                if (ModelState.IsValid == false)
                    return BadRequest(ModelState);
                var userId = new UserClaimHelper(User).GetUserIdSL();
                var result = await _editor.EditServerProfile(userId,new ServerId(  request.ServerId),"",request.File);
                if (result.Failure)
                    return BadRequest(result.ErrorMessage);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpPost("Profile")]
        [RequestSizeLimit(6 * 1024 * 1024)]
        public async Task<IActionResult> UpdateProfile([FromForm] NewChatServerProfileRequest request)
        {
            try
            {
                if (ModelState.IsValid == false)
                    return BadRequest(ModelState);
                var userId = new UserClaimHelper(User).GetUserIdSL();
                var result = await _editor.EditServerProfile(userId, new ServerId(request.ServerId), request.Name, request.File);
                if (result.Failure)
                    return BadRequest(result.ErrorMessage);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
