using iChat.BackEnd.Services.Users.Infra.Neo4j;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace iChat.BackEnd.Controllers.UserControllers.MessageControllers
{
    [Route("Chat")]
    [Authorize]
    public class ChannelServerCreateController : Controller
    {
        private readonly CreateChatService _service;
        public ChannelServerCreateController(CreateChatService service)
        {
            _service = service;
        }
        [HttpGet("Create")]
        public IActionResult Create()
        {
            return View();
        }
        [HttpPost("Create")]
        public async Task<IActionResult> CreateServer(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                ModelState.AddModelError("Name", "Server name is required.");
                return View();
            }

            var userId = new UserClaimHelper(User).GetUserId();
            await _service.CreateServerAsync(name, userId);
            return RedirectToAction("Index", "ChatServer");
        }
        [HttpGet("{id}/CreateChannel")]
        public IActionResult CreateChannel(long id)
        {
            ViewData["ServerId"] = id;
            return PartialView("_CreateChannel"); 
        }

        [HttpPost("{id}/CreateChannel")]
        public async Task<IActionResult> CreateChannel(long id, string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return BadRequest("Channel name is required.");
            }

            var userId = new UserClaimHelper(User).GetUserId();
            var channelId = await _service.CreateChannelAsync(id, name, userId);

            if (channelId == -1)
                return BadRequest("Failed to create channel.");

            return Ok();
        }

    }
}
