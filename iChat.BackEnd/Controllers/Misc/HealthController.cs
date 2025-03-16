using iChat.BackEnd.Services.Users.Infra.CassandraDB;
using Microsoft.AspNetCore.Mvc;
using Neo4j.Driver;

namespace iChat.BackEnd.Controllers
{
    [Route("api/health")]
    [ApiController]
    public class HealthController : ControllerBase
    {
        private readonly IDriver _neo4jDriver;
        private readonly MessageUpdateService _messageEditService;

        public HealthController(IDriver neo4jDriver,MessageUpdateService messageEditService)
        {
            _messageEditService = messageEditService;
            _neo4jDriver = neo4jDriver;
        }
        [HttpGet("Cassandra")]
        public async Task<IActionResult> CheckCassandra()
        {
            try
            {

                if (await _messageEditService.Health())
                    return Ok(new { status = "Healthy", message = "Connected to Neo4j successfully" });
                return StatusCode(500, new { status = "Unhealthy" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { status = "Unhealthy", error = ex.Message });
            }
        }

        [HttpGet("neo4j")]
        public async Task<IActionResult> CheckNeo4j()
        {
            try
            {
                await using var session = _neo4jDriver.AsyncSession();
                var result = await session.ExecuteReadAsync(tx =>
                    tx.RunAsync("RETURN 'Neo4j is up' AS message"));

                return Ok(new { status = "Healthy", message = "Connected to Neo4j successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { status = "Unhealthy", error = ex.Message });
            }
        }
    }
}
