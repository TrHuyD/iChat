using iChat.BackEnd.Services.Users;
using iChat.BackEnd.Services.Users.Auth;
using iChat.BackEnd.Services.Users.ChatServers;
using iChat.BackEnd.Services.Users.ChatServers.Application;
using iChat.BackEnd.Services.Users.Infra.MemoryCache;
using iChat.Data.Entities.Users;
using iChat.DTOs.Users;
using iChat.DTOs.Users.Messages;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using StackExchange.Redis;
using System.Security.Claims;
using System.Threading.Tasks;

namespace iChat.BackEnd.Controllers
{
    [Route("api/users")]
    [ApiController]
    public class UserApiController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserApiController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet("profile")]
        [Authorize]
        public async Task<IActionResult> GetProfile()
        {
            var userId = new UserClaimHelper(User).GetUserIdStr();

            var userProfile = await _userService.GetUserProfileAsync(userId);
            if (userProfile == null)
                return NotFound();

            return Ok(userProfile);
        }
        [HttpGet("CompleteInfo")]
        [Authorize]
        public async Task<IActionResult> GetCompleteInfo([FromServices] ServerListService serverListService, [FromServices] IMemoryCache _cache, [FromServices] MemCacheUserChatService metadatacache,
            [FromServices] AppUserService metadataProvider)
        {
            var userId = new UserClaimHelper(User).GetUserId();
            var userIdStr = new UserClaimHelper(User).GetUserIdStr();

            var cacheKey = $"complete_info:{userId}";
            if (!_cache.TryGetValue(cacheKey, out UserCompleteDto? package))
            {
                // var userProfile = await _userService.GetUserProfileAsync(userId.ToString());
                var metatdata =await metadataProvider.GetUserMetadataAsync(userIdStr);
                var userServerList = await serverListService.GetServerList(userId);
                metadatacache.SetOnlineUserData( userServerList.Select(t=>long.Parse(t.Id)).ToList(),metatdata);
                package = new UserCompleteDto
                {
                    UserProfile = metatdata,
                    ChatServers = userServerList
                };
                _cache.Set(cacheKey, package, TimeSpan.FromSeconds(10));
            }
            return package is null ? NotFound() : Ok(package);
        }


    }

}