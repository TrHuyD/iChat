using Neo4j.Driver;

namespace iChat.BackEnd.Services.StartUpServices.SUS_ChatServer
{
    class SUS_Neo4jServerLister
    {
        private readonly Lazy<IAsyncSession> __session;
        public SUS_Neo4jServerLister(Lazy<IAsyncSession> session)
        {
            __session = session;
        }
        public async Task<Dictionary<long, List<long>>> GetAllServerChannelsAsync()
        {
            var query = @"
                MATCH (s:ChatServer)-[:HAS_CHANNEL]->(c:ChatChannel)
                RETURN s.id AS serverId, collect(c.id) AS channelIds
            ";

            var session = __session.Value;
            var result = await session.RunAsync(query);

            var dict = new Dictionary<long, List<long>>();
            await result.ForEachAsync(record =>
            {
                var serverId = record["serverId"].As<long>();
                var channelIds = record["channelIds"].As<List<long>>();
                dict[serverId] = channelIds;
            });

            return dict;
        }

    }
}
