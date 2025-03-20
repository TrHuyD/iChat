using Neo4j.Driver;

namespace iChat.BackEnd.Services.Users.Infra.Neo4jService
{
    public class Neo4jChatChannelEditService
    {
        private readonly IAsyncSession _session;

        public Neo4jChatChannelEditService(IAsyncSession session)
        {
            _session = session;
        }

        public async Task<bool> EditChatChannelNameAsync(string channelId, string newName)
        {
            var query = "MATCH (c:ChatChannel {id: $channelId}) SET c.name = $newName";
            try
            {
                await _session.RunAsync(query, new { channelId, newName });
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error editing chat channel name: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> DeleteChatChannelAsync(string channelId)
        {
            var query = "MATCH (c:ChatChannel {id: $channelId}) DETACH DELETE c";
            try
            {
                await _session.RunAsync(query, new { channelId });
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting chat channel: {ex.Message}");
                return false;
            }
        }
    }
}
