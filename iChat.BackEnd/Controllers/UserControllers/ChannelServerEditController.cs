//using iChat.BackEnd.Services.Users.Infra.IdGenerator;

//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Mvc;

//namespace iChat.BackEnd.Controllers.UserControllers
//{
//    [Authorize]
//    [Route("Chat")]
//    public class ChatServerEditController : Controller
//    {
  
//        private readonly ChannelIdService _IdGen;

//        public ChatServerEditController(ChannelIdService IdGen)
//        {
//            _IdGen = IdGen;
//            _chatServerService = chatServerService;
//        }
//        //[HttpGet]
//        //public IActionResult Create()
//        //{
//        //    return View();
//        //}

//        //[HttpPost]
//        //public async Task<IActionResult> Create(string name)
//        //{
//        //    if (string.IsNullOrWhiteSpace(name))
//        //    {
//        //        ModelState.AddModelError("Name", "Server name is required.");
//        //        return View();
//        //    }

//        //    var serverId = _IdGen.GenerateId();
//        //    var userId = new UserClaimHelper(User).GetUserId();
//        //    await _chatServerService.CreateChatServerAsync(serverId, name, userId);

//        //    return RedirectToAction("Index", "ChatServer");
//        //}

//        [HttpGet("{id}\\Edit")]

//        public async Task<IActionResult> Edit(string id)
//        {
//            return View(new { ServerId = id });
//        }

//        [HttpPost("{id}\\Edit")]

//        public async Task<IActionResult> Edit(string id, string newName)
//        {
//            if (string.IsNullOrWhiteSpace(newName))
//            {
//                ModelState.AddModelError("Name", "Server name is required.");
//                return View(new { ServerId = id });
//            }
//            var userId = new UserClaimHelper(User).GetUserIdStr();
//            await _chatServerService.UpdateChatServerNameAsync(id, newName, userId);

//            return RedirectToAction("Index", "ChatServer");
//        }


//        [HttpPost("{id}\\Delete")]
//        public async Task<IActionResult> Delete(string id)
//        {
//            var userId = new UserClaimHelper(User).GetUserIdStr();
//            await _chatServerService.DeleteChatServerAsync(id, userId);
//            return RedirectToAction("Index", "ChatServer");
//        }
//        [HttpPost("{serverId}\\{channelId}")]
//        public IActionResult ViewChannel(string serverId, string channelId)
//        {
//            ViewData["ServerId"] = serverId;
//            ViewData["ChannelId"] = channelId;
//            return View();
//        }
//    }
//}
