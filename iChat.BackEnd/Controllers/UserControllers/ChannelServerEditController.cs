using iChat.BackEnd.Services.Users.Infra.Neo4j;
using Microsoft.AspNetCore.Mvc;

namespace iChat.BackEnd.Controllers.UserControllers
{
    public class ChatServerEditController : Controller
    {
        private readonly ChatServerService _chatServerService;

        public ChatServerEditController(ChatServerService chatServerService)
        {
            _chatServerService = chatServerService;
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                ModelState.AddModelError("Name", "Server name is required.");
                return View();
            }

            string serverId = Guid.NewGuid().ToString();
            await _chatServerService.CreateChatServerAsync(serverId, name);
            return RedirectToAction("Index", "ChatServer");
        }

        public async Task<IActionResult> Edit(string id)
        {
            return View(new { ServerId = id });
        }

        [HttpPost]
        public async Task<IActionResult> Edit(string id, string newName)
        {
            if (string.IsNullOrWhiteSpace(newName))
            {
                ModelState.AddModelError("Name", "Server name is required.");
                return View(new { ServerId = id });
            }

            await _chatServerService.EditChatServerNameAsync(id, newName);
            return RedirectToAction("Index", "ChatServer");
        }

        [HttpPost]
        public async Task<IActionResult> Delete(string id)
        {
            await _chatServerService.DeleteChatServerAsync(id);
            return RedirectToAction("Index", "ChatServer");
        }

        public IActionResult ViewChannel(string serverId, string channelId)
        {
            ViewData["ServerId"] = serverId;
            ViewData["ChannelId"] = channelId;
            return View();
        }
    }
}
