using Auth0.ManagementApi.Models;
using iChat.BackEnd.Models.MessageSearch;
using iChat.BackEnd.Services.Users.ChatServers.Abstractions;
using iChat.Data.Entities.Users.Messages;
using iChat.DTOs.Users.Messages;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text.RegularExpressions;

namespace iChat.BackEnd.Controllers.UserControllers.MessageControllers
{
    namespace iChat.BackEnd.Controllers
    {
        [ApiController]
        [Route("api/search")]
        [Authorize]
        public class SearchController : ControllerBase
        {
            private readonly IMessageSearchService _searchService;
            private readonly ILogger<SearchController> _logger;

            public SearchController(IMessageSearchService searchService, ILogger<SearchController> logger)
            {
                _searchService = searchService;
                _logger = logger;
            }
            [HttpGet("messages")]
            public async Task<IActionResult> SearchMessages([FromQuery] SearchOptions request)
            {
                try
                {
                    var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                    if (string.IsNullOrEmpty(userId))
                    {
                        return Unauthorized();
                    }
                    if (request.ChannelId == null && request.ServerId == null)
                    {
                        return BadRequest("Either ChannelId or ServerId must be provided.");
                    }
                    var options = new SearchOptions
                    {
                        QueryText = request.QueryText,
                        ChannelId = request.ChannelId,
                        ServerId = request.ServerId,
                        SenderId = request.SenderId,
                        FromDate = request.FromDate,
                        ToDate = request.ToDate,
                        Page = request.Page,
                        PageSize = Math.Min(request.PageSize, 50),
                        SortBy = request.SortBy ?? "timestamp",
                        SortDescending = request.SortDescending,
                        CursorToken = request.CursorToken,
                    };
                    var result = await _searchService.SearchMessagesAsync(options);
                    return Ok(result);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error searching messages for user {UserId}", User.Identity?.Name);
                    return StatusCode(500, "An error occurred while searching messages");
                }
            }

            [HttpGet("count")]
            public async Task<IActionResult> GetSearchCount([FromQuery] string query, [FromQuery] long? channelId, [FromQuery] long? serverId)
            {
                try
                {
                    if (string.IsNullOrWhiteSpace(query))
                        return BadRequest("Search query cannot be empty");

                    if (!channelId.HasValue && !serverId.HasValue)
                        return BadRequest("Either Channel ID or Server ID is required");

                    var options = new SearchOptions
                    {
                        QueryText = query,
                        ChannelId = channelId,
                        ServerId = serverId,
                    };

                    var count = await _searchService.GetSearchResultCountAsync(query, channelId ?? 0, options);
                    return Ok(new { count });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error getting search count");
                    return StatusCode(500, "An error occurred while getting search count");
                }
            }



        }
    }
}