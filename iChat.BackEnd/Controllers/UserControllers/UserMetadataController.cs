using iChat.BackEnd.Services.Users.ChatServers;
using iChat.DTOs.Users.personal;
using Microsoft.AspNetCore.Mvc;

namespace iChat.BackEnd.Controllers.UserControllers
{
    [Route("api/users")]
    public class UserMetadataController:ControllerBase
 
    {
        private readonly UserApplicationService _userMetadataService;
        public UserMetadataController(UserApplicationService userMetadataService)
        {
            _userMetadataService = userMetadataService;
        }
        [HttpGet("GetUserById")]
        public async Task<IActionResult> GetUserMetadata([FromQuery] string userId)
        {
            if (string.IsNullOrEmpty(userId) || !long.TryParse(userId, out var luserId) || luserId <= 0)
                return BadRequest("User ID is required.");
            var metadata = await _userMetadataService.GetUserMetadataAsync(userId);
            if (metadata == null)
                return NotFound();
            Response.Headers["Cache-Control"] = "public,max-age=86400"; 
            Response.Headers["Expires"] = DateTime.UtcNow.AddDays(1).ToString("R"); 
            return Ok(metadata);
        }
        [HttpPost("GetUsersByIds")]
        public async Task<IActionResult> GetUsersMetadata([FromBody] List<string> userIds)
        {
            if (userIds == null || userIds.Count == 0)
                return BadRequest("User IDs are required.");
            var metadataList = await _userMetadataService.GetUserMetadataBatchAsync(userIds);
            if (metadataList == null || metadataList.Count == 0)
                return NotFound();
            return Ok(metadataList);
        }
        [HttpPost("UpdateUserName")]
        public async Task<IActionResult> UpdateUserName([FromBody] UpdateNicknameRequest request)
        {
            if(ModelState.IsValid == false)
                return BadRequest(ModelState);
            var userId = new UserClaimHelper(User).GetUserIdStr();
            var updatedMetadata = await _userMetadataService.UpdateUserName(userId, request.NewNickName);
            return Ok(updatedMetadata);
        }

    }

}
