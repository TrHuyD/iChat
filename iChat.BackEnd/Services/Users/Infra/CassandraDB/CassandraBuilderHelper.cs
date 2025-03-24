using iChat.BackEnd.Models.Helpers.CassandraOptionss;
using iChat.BackEnd.Models.Infrastructures;
using iChat.BackEnd.Services.Users.Infra.IdGenerator;

namespace iChat.BackEnd.Services.Users.Infra.CassandraDB
{
    public class CassandraBuilderHelper
    {
         public void AddService(WebApplicationBuilder builder)
        {
            var CassandraRWConfig = new CassandraOptions(builder.Configuration.GetSection("Cassandra:ReadWriteOnly"));
            builder.Services.AddSingleton<CasandraService>(provider =>
            {
                return new CasandraService(CassandraRWConfig, provider.GetRequiredService<ILogger<CasandraService>>());
            });
            builder.Services.AddSingleton<CassandraMessageWriteService>();
        }
    }
}
