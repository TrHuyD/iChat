using Neo4j.Driver;

namespace iChat.BackEnd.Services.Users.Infra.Neo4jService
{
    public class Neo4jChatServerEditService
    {
        private readonly IAsyncSession _session;

        public Neo4jChatServerEditService(IAsyncSession session)
        {
            _session = session;
        }
        public async Task<bool> UpdateChatServerNameAsync(string serverId, string newName, string adminUserId)
        {
            var query = @"
                        MATCH (u:User {id: $adminUserId})-[:ADMIN_OF]->(s:ChatServer {id: $serverId}) 
                        SET s.name = $newName";

            try
            {
                var result = await _session.RunAsync(query, new { serverId, newName, adminUserId });
                return result.ConsumeAsync().IsCompletedSuccessfully;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating chat server name: {ex.Message}");
                return false;
            }
        }


        public async Task<bool> DeleteChatServerAsync(string serverId, string adminUserId)
        {
            var query = @"
                MATCH (u:User {id: $adminUserId})-[:ADMIN_OF]->(s:ChatServer {id: $serverId}) 
                DETACH DELETE s";

            try
            {
                var result = await _session.RunAsync(query, new { serverId, adminUserId });
                return result.ConsumeAsync().IsCompletedSuccessfully;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting chat server: {ex.Message}");
                return false;
            }
        }





    }

}
