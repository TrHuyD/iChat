using iChat.BackEnd.Services.Users.Infra.CassandraDB;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace iChat.BackEnd.Controllers.UserControllers.MessageControllers
{
    [ApiController]
    [Route("api/message/send")]
    [Authorize]
    public class MessageSendController :ControllerBase
    {
        private readonly MessageWriteService _messageWriteService;
        public MessageSendController(MessageWriteService messageWriteService)
        {
            _messageWriteService = messageWriteService;
        }

    }
}
