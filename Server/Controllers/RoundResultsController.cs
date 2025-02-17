using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VgaUI.Shared;

namespace VgaUI.Server.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class RoundResultsController : ControllerBase
    {
        private readonly ILogger<RoundResultsController> _logger;
        private readonly MongoDBService _db;
        private const string USER_ERR_MSG = "RoundResultsController - An error occurred while processing your request. Check the server logs.";

        public RoundResultsController(ILogger<RoundResultsController> logger, MongoDBService mongoDBService)
        {
            _logger = logger;
            _db = mongoDBService;
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] RoundResults value)
        {
            if (value == null)
            {
                return BadRequest("RoundResults cannot be null");
            }

            try
            {
                string? idFromDB = await _db.GetID(value.CourseName, value.DateOfPlay);
            if (value.Id != null || (idFromDB == null) || idFromDB == value.Id)
                {
                    RoundResults roundResult = await _db.AddOrReplaceRoundResultsAsync(value);
                    return Ok(roundResult);
                }
                return BadRequest("Invalid ID provided.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred in Post: {Message}", ex.Message);
                return StatusCode(500, USER_ERR_MSG);
            }
        }

        [HttpGet("latest")]
        public async Task<IActionResult> GetLatest()
        {
            try
            {
                var latestRound = await _db.GetLastRoundResultsAsync();
                _logger.LogInformation("Fetched Id {Id} from the database.", latestRound.Id);
                return Ok(latestRound);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred in GetLatest: {Message}", ex.Message);
                return StatusCode(500, USER_ERR_MSG);
            }
        }

        [AllowAnonymous]
        [HttpGet("list")]
        public async Task<IActionResult> GetRoundList(int limit)
        {
            try
            {
                var roundList = await _db.GetRoundList(limit);
                _logger.LogInformation("Fetched {Count} rounds from the database.", roundList.Count);
                return Ok(roundList);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred in GetRoundList: {Message}", ex.Message);
                return StatusCode(500, USER_ERR_MSG);
            }
        }

        [AllowAnonymous]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetId([FromRoute] string id)
        {
            try
            {
                _logger.LogInformation("Attempting to fetch Id {Id} from the database.", id);
                var result = await _db.GetRoundResultsAsync(id) ?? new RoundResults();
                _logger.LogInformation("Fetched Id {Id} from the database.", id);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred in GetId: {Message}", ex.Message);
                return StatusCode(500, USER_ERR_MSG);
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteId([FromRoute] string id)
        {
            try
            {
                _logger.LogInformation("Attempting to delete Id {Id} from the database.", id);
                await _db.DeleteRoundResultsAsync(id);
                _logger.LogInformation("Deleted Id {Id} from the database.", id);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred in DeleteId: {Message}", ex.Message);
                return StatusCode(500, USER_ERR_MSG);
            }
        }
    }
}
