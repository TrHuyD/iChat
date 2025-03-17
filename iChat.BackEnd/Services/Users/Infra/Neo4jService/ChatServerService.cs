using Neo4j.Driver;

namespace iChat.BackEnd.Services.Users.Infra.Neo4jService
{
    public class ChatServerService
    {
        private readonly IAsyncSession _session;

        public ChatServerService(IAsyncSession session)
        {
            _session = session;
        }

        //public async Task<bool> CreateChatServerAsync(long serverId, string name, long adminUserId)
        //{
        //    var query = @"
        //                CREATE (s:ChatServer {id: $serverId, name: $name})
        //                WITH s
        //                MATCH (u:User {id: $adminUserId})
        //                CREATE (u)-[:ADMIN_OF]->(s)";

        //    try
        //    {
        //        await _session.RunAsync(query, new { serverId, name, adminUserId });
        //        return true;
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine($"Error creating chat server: {ex.Message}");
        //        return false;
        //    }
        //}

        public async Task<bool> UpdateChatServerNameAsync(long serverId, string newName, long adminUserId)
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


        public async Task<bool> DeleteChatServerAsync(long serverId, long adminUserId)
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


        //public async Task<bool> CreateChatChannelAsync(long serverId, long channelId, string channelName)
        //{
        //    var query = @"MATCH (s:ChatServer {id: $serverId})
        //                 CREATE (c:ChatChannel {id: $channelId, name: $channelName})-[:BELONGS_TO]->(s)";
        //    try
        //    {
        //        await _session.RunAsync(query, new { serverId, channelId, channelName });
        //        return true;
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine($"Error creating chat channel: {ex.Message}");
        //        return false;
        //    }
        //}

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
        public async Task<long?> GetDefaultChannelId(long userId, long serverId)
        {
            IDriver driver = GraphDatabase.Driver("bolt://localhost:7687", AuthTokens.Basic("neo4j", "password"));

            using (var session = driver.AsyncSession())
            {
                try
                {
                    var result = await session.RunAsync(@"
                        MATCH (u:User {id: $userId})-[:MEMBER_OF]->(s:ChatServer {id: $serverId})
                        MATCH (s)-[:DEFAULT_CHANNEL]->(c:Channel)
                        RETURN c.id AS DefaultChannelId",
                        new { userId, serverId });
                    if (await result.FetchAsync())
                    {
                        return long.Parse(result.Current["DefaultChannelId"].As<string>());
                    }

                    return null; 
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error retrieving default channel: {ex.Message}");
                    return null;
                }
            }
        }


    }

}
