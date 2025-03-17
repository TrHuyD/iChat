using Neo4j.Driver;

namespace iChat.BackEnd.Services.Users.Infra.Neo4jService
{
    public class Neo4jCreateUserService
    {
        private readonly IAsyncSession _session;
        public Neo4jCreateUserService(IAsyncSession session)
        {
            _session = session;
        }
        public async Task CreateUserNode(long id)
        {
            await _session.ExecuteWriteAsync(tx =>
                tx.RunAsync("MERGE (u:User {id: $id})", new { id })
            );
        }
    }
}
