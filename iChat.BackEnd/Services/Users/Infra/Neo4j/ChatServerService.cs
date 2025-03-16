using Neo4j.Driver;

namespace iChat.BackEnd.Services.Users.Infra.Neo4j
{
    public class ChatServerService
    {
        private readonly IAsyncSession _session;

        public ChatServerService(IAsyncSession session)
        {
            _session = session;
        }

        public async Task<bool> CreateChatServerAsync(string serverId, string name)
        {
            var query = "CREATE (s:ChatServer {id: $serverId, name: $name})";
            try
            {
                await _session.RunAsync(query, new { serverId, name });
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating chat server: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> EditChatServerNameAsync(string serverId, string newName)
        {
            var query = "MATCH (s:ChatServer {id: $serverId}) SET s.name = $newName";
            try
            {
                await _session.RunAsync(query, new { serverId, newName });
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error editing chat server name: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> DeleteChatServerAsync(string serverId)
        {
            var query = "MATCH (s:ChatServer {id: $serverId}) DETACH DELETE s";
            try
            {
                await _session.RunAsync(query, new { serverId });
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting chat server: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> CreateChatChannelAsync(string serverId, string channelId, string channelName)
        {
            var query = @"MATCH (s:ChatServer {id: $serverId})
                         CREATE (c:ChatChannel {id: $channelId, name: $channelName})-[:BELONGS_TO]->(s)";
            try
            {
                await _session.RunAsync(query, new { serverId, channelId, channelName });
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating chat channel: {ex.Message}");
                return false;
            }
        }

        public async Task<List<string>> GetUserAccessibleChannelsAsync(string userId)
        {
            var query = @"MATCH (u:User {id: $userId})-[:HAS_ROLE]->(:Role)-[:GRANTS_ACCESS]->(c:ChatChannel)
                         RETURN c.id AS channelId";
            try
            {
                var result = await _session.RunAsync(query, new { userId });
                return await result.ToListAsync(r => r["channelId"].As<string>());
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving user accessible channels: {ex.Message}");
                return new List<string>();
            }
        }
    }

}
