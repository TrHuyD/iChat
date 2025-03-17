using iChat.BackEnd.Models.Helpers;
using iChat.BackEnd.Models.User.MessageRequests;
using iChat.BackEnd.Services.Users.Infra.IdGenerator;
using Microsoft.AspNetCore.Mvc;

namespace iChat.BackEnd.Services.Users.Infra.CassandraDB
{
    public class MessageWriteService :CasandraService
    {
        private readonly SnowflakeService idGen;
        public MessageWriteService(CassandraOptions options,SnowflakeService snowflakeService) : base(options)
        {
            idGen = snowflakeService;
        }
       public async Task<IActionResult> WriteTextMessageAsync(MessageRequest rq)
        {
            throw new NotImplementedException();
        }
    }
}
