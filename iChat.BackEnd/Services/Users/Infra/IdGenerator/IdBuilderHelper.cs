using iChat.BackEnd.Models.Infrastructures;

namespace iChat.BackEnd.Services.Users.Infra.IdGenerator
{
    public  class IdBuilderHelper
    {
         public void AddService(WebApplicationBuilder builder,WorkerID id)
        {
            builder.Services.AddSingleton(new SnowflakeService(id.WorkerId,id.DataCenterId));
            builder.Services.AddSingleton(new UserIdService(id.WorkerId, id.DataCenterId));
            builder.Services.AddSingleton(new ServerIdService(id.WorkerId, id.DataCenterId));
            builder.Services.AddSingleton(new ChannelIdService(id.WorkerId, id.DataCenterId));
        }
    }
}
