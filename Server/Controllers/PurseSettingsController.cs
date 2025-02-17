using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VgaUI.Shared;

namespace VgaUI.Server.Controllers
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class PurseSettingsController : ControllerBase
    {
        private readonly MongoDBService _db;
        private readonly ILogger<PurseSettingsController> _logger;
        public PurseSettingsController(MongoDBService db, ILogger<PurseSettingsController> logger)
        {
            _db = db;
            _logger = logger;
        }
        [HttpGet("{name}")]
        public async Task<ActionResult<PurseSettings>> Get([FromRoute]string name)
        {
            var result = await _db.GetPurseSettingsAsync(name);
            if (result == null)
            {
                return NotFound();
            }
            return Ok(result);
        }

        // POST api/<PurseSettingsController>
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] PurseSettings value)
        {
            if (value == null)
            {
                return BadRequest("PurseSettings cannot be null");
            }

            try
            {
                await _db.StorePurseSettingsAsync(value);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating purse settings");
                return StatusCode(500, "Internal server error");
            }
        }

        // PUT api/<PurseSettingsController>/5
        [HttpPut]
        public async Task<IActionResult> Put([FromBody] PurseSettings value)
        {
            if (value == null)
            {
                return BadRequest("PurseSettings cannot be null");
            }

            try
            {
                await _db.StorePurseSettingsAsync(value);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating purse settings");
                return StatusCode(500, "Internal server error");
            }
        }

        // DELETE api/<PurseSettingsController>/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            try
            {
                await _db.DeletePurseSettingsAsync(id);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting purse settings with id {Id}", id);
                return StatusCode(500, "Internal server error");
            }
        }
    }
}
