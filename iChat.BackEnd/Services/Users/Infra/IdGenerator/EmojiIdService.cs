using iChat.BackEnd.Models.Infrastructures;

namespace iChat.BackEnd.Services.Users.Infra.IdGenerator
{
    public class EmojiIdService:SnowflakeService
    {
        public EmojiIdService(int workerId, int datacenterId) : base(workerId, datacenterId, 1754896379000L)
        {

        }
}
}
