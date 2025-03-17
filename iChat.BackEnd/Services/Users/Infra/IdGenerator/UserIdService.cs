namespace iChat.BackEnd.Services.Users.Infra.IdGenerator
{
    public class UserIdService:SnowflakeService
    {
        public UserIdService(int workerId, int datacenterId) : base(workerId, datacenterId, 1742148859000L)
        {
            
        }
    }
}
