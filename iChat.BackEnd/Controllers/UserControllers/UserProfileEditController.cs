using iChat.BackEnd.Models.User;
using iChat.BackEnd.Services.Users.ChatServers;
using iChat.BackEnd.Services.Users.ChatServers.Application;
using Microsoft.AspNetCore.Mvc;

namespace iChat.BackEnd.Controllers.UserControllers
{
    [Route("api/users/Update")]
    public class UserProfileEditController:ControllerBase
    {
        AppUserEditService _editService;
        ChatHubResponer _responder;

        public UserProfileEditController(AppUserEditService editService,ChatHubResponer responder)
        {
            _editService = editService;
            _responder = responder;
        }
        [HttpPost("NickName")]

        public async Task<IActionResult> UpdateUserName([FromBody] NewNickName request)
        {

            if (ModelState.IsValid == false)
                return BadRequest(ModelState);
            try
            {
                var userId = new UserClaimHelper(User).GetUserIdStr();
                var updatedMetadata = await _editService.UpdateUserName(userId, request.UserName);
                await _responder.UpdateProfile(updatedMetadata);
                return Ok();
            }
            catch(Exception ex)
            {
                return BadRequest(ex.Message);
            }
            }
        [HttpPost("Avatar")]
        [RequestSizeLimit(6 * 1024 * 1024)]
        public async Task<IActionResult> UpdateAvatar([FromForm] NewAvatarRequest request)
        {
            try
            {
                if (ModelState.IsValid == false)
                    return BadRequest(ModelState);
                var userId = new UserClaimHelper(User).GetUserIdStr();
                var updatedMetadata = await _editService.UpdateUserAvatar(userId, request.File);
                await _responder.UpdateProfile(updatedMetadata);
                return Ok();
            }
                        catch(Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpPost("Profile")]
        [RequestSizeLimit(6 * 1024 * 1024)]
        public async Task<IActionResult> UpdateProfile([FromForm] NewProfileRequest request)
        {
            try
            {
                if (ModelState.IsValid == false)
                    return BadRequest(ModelState);
                var userId = new UserClaimHelper(User).GetUserIdStr();
                var updatedMetadata = await _editService.UpdateUserNameAndAvatar(userId, request.UserName, request.File);
                await _responder.UpdateProfile(updatedMetadata);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


    }
}
