using iChat.BackEnd.Models.Helpers.CassandraOptionss;

namespace iChat.BackEnd.Services.Users.Infra.CassandraDB
{
    public class MessageUpdateService :CasandraService
    {
       public  MessageUpdateService(CassandraOptions options) : base(options)
        {

        }
    }
}
