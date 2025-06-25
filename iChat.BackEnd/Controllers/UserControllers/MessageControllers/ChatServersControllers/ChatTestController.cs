using iChat.BackEnd.Services.Users.Infra.Redis.ChatServerServices;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace iChat.BackEnd.Controllers.UserControllers.MessageControllers.ChatServersControllers
{
    [Route("api/ChatTest")]
    [ApiController]
    public class ChatTestController: ControllerBase
    {
        
        [HttpGet("test")]
        public IActionResult TestEndpoint()
        {
            return Ok("ChatTestController is working!");
        }
        [HttpGet("servermetadata/{id}")]
        public async Task<IActionResult> TestEndpointWithId([FromServices] RedisChatServerService _test,string id)
        {
            return Ok(await _test.GetServerFromRedisAsync(id));
        }
    }
}
