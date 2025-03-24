using iChat.BackEnd.Services.Users.Infra.CassandraDB;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace iChat.BackEnd.Controllers.UserControllers.MessageControllers
{
    [ApiController]
    [Route("chat")]
    [Authorize]
    public class MessageSendController :ControllerBase
    {
        private readonly CassandraMessageWriteService _messageWriteService;
        public MessageSendController(CassandraMessageWriteService messageWriteService)
        {
            _messageWriteService = messageWriteService;
        }

    }
}
