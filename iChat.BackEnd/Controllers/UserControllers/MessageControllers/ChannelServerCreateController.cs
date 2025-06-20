﻿using iChat.BackEnd.Services.Users.ChatServers.Abstractions;
using iChat.BackEnd.Services.Users.Infra.Neo4jService;
using iChat.DTOs.Users.Messages;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace iChat.BackEnd.Controllers.UserControllers.MessageControllers
{
    [Route("api/Chat")]
    [Authorize]
    [ApiController]
    public class ChannelServerCreateController : ControllerBase
    {
        private readonly IChatCreateService _service;
        public ChannelServerCreateController(IChatCreateService service)
        {
            _service = service;
        }

        [HttpPost("Create")]
        public async Task<IActionResult> CreateServer([FromBody] ChatServerCreateRq rq)
        {
            var name = rq.Name;
            if (string.IsNullOrWhiteSpace(name))
            {
                ModelState.AddModelError("Name", "Server name is required.");
                return BadRequest(ModelState);
            }

            var userId = new UserClaimHelper(User).GetUserId();
            var serverid= await _service.CreateServerAsync(name, userId);
            return Ok(serverid);
        }
        //[HttpGet("{id}/CreateChannel")]
        //public IActionResult CreateChannel(long id)
        //{
        //    ViewData["ServerId"] = id;
        //    return PartialView("_CreateChannel"); 
        //}

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
