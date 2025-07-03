using iChat.BackEnd.Models.User.MessageRequests;
using iChat.BackEnd.Services.Users.ChatServers.Abstractions;
using iChat.BackEnd.Services.Users.Infra.Redis.MessageServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Channels;

namespace iChat.BackEnd.Controllers.UserControllers.MessageControllers.ChatServersControllers
{
    [Route("api/ChatChannel")]
    [ApiController]
    [Authorize]
    public class ChatReadMessageController : ControllerBase
    {
        private readonly IChatReadMessageService _chatReadMessageService;
        public ChatReadMessageController(IChatReadMessageService chatReadMessageService)
        {
            _chatReadMessageService = chatReadMessageService;
        }

        [HttpGet("{channelId}/read")]
        public async Task<IActionResult> ReadMessages(string channelId)
        {
            var userId = new UserClaimHelper(User).GetUserId();
            if (long.TryParse(channelId, out long channelIdLong) == false || (channelIdLong <= 1000000000000000l))
            {
                return BadRequest("Invalid channel ID format.");
            }
            var request = new UserGetRecentMessageRequest
            {
                ChannelId = channelIdLong,
                UserId = userId
            };
            var messages = await _chatReadMessageService.RetrieveRecentMessage(request);
            return Ok(messages);
        }
        [HttpGet("{channelId}/history")]
        public async Task<IActionResult> GetMessageHistory(string channelId, [FromQuery] string? beforeMessageId = null)
        {
            if (long.TryParse(channelId, out long channelIdLong) == false || (channelIdLong <= 1000000000000000l))
            {
                return BadRequest("Invalid channel ID format.");
            }

            if (!string.IsNullOrEmpty(beforeMessageId) && long.TryParse(beforeMessageId, out var beforeMessageIdLong))
            {

            }
            else
                return BadRequest("Invalid Message ID format.");
            var messages = await _chatReadMessageService.GetMessagesBeforeAsync(channelIdLong, beforeMessageIdLong);
            return Ok(messages);
        }

        [HttpGet("{channelId:long}/buckets")]
        public async Task<IActionResult> GetBucketsInRange(
            [FromServices] IMessageDbReadService readService,
            long channelId,
            [FromQuery(Name = "endid")] int endId,
            [FromQuery(Name = "limit")] int limit)
        {
            if (channelId <= 1_000_000_000_000_000)
                return BadRequest("Invalid channel ID.");
            if (endId < 0 || limit <= 0 || limit > 3)
                return BadRequest("Invalid bucket ID range.");
            var buckets = await readService.GetBucketsInRangeAsync(channelId, endId - limit + 1, endId, 150);
            return Ok(buckets);
        }
        [HttpGet("{channelId}/latest")]
        public async Task<IActionResult> test([FromServices] IMessageDbReadService readService, string channelId)
        {
            if (long.TryParse(channelId, out long channelIdLong) == false || (channelIdLong <= 1000000000000000l))
            {
                return BadRequest("Invalid channel ID format.");
            }
            return Ok(await readService.GetLatestBucketsByChannelAsync(channelIdLong));
        }
        [HttpGet("{channelId:long}/bucketsingle")]
        public async Task<IActionResult> GetBucketById(
            [FromServices] IMessageDbReadService readService,
            long channelId,
            [FromQuery(Name = "id")] int bucketId)
        {
            if (channelId <= 1_000_000_000_000_000)
                return BadRequest("Invalid channel ID.");
            if (bucketId < 0)
                return BadRequest("Invalid bucket ID.");
            var bucket = await readService.GetBucketById(channelId, bucketId);
            return Ok(bucket);
        }
    }
}