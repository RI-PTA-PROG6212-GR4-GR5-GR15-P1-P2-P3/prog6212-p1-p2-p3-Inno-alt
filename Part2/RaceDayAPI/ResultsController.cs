using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDayAPI.Data;
using RaceDayAPI.Models;
using System.Security.Claims;

namespace RaceDayAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ResultsController : ControllerBase
    {
        private readonly RaceDayDbContext _context;

        public ResultsController(RaceDayDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Get a specific result by ID (Participant self or Organiser)
        /// </summary>
        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetResult(int id)
        {
            var currentUserId = GetCurrentUserId();
            var currentRole = GetCurrentUserRole();

            var result = await _context.Results
                .Include(r => r.Entry)
                    .ThenInclude(e => e!.Participant)
                .Include(r => r.Entry)
                    .ThenInclude(e => e!.Event)
                .FirstOrDefaultAsync(r => r.ResultID == id);

            if (result == null)
            {
                return NotFound(new { error = "Result not found." });
            }

            // Check access: participant owner or organiser
            if (currentRole != "Organiser" && result.Entry?.Participant?.UserID != currentUserId)
            {
                return Forbid();
            }

            return Ok(new
            {
                result.ResultID,
                result.FinishTime,
                result.Position,
                result.GunTime,
                result.ChipTime,
                result.Pace,
                result.Status,
                result.OverallPosition,
                result.AgeGroupPosition,
                result.GenderPosition,
                result.CertificateURL,
                ParticipantName = result.Entry?.Participant?.FullName,
                EventName = result.Entry?.Event?.EventName,
                BibNumber = result.Entry?.BibNumber
            });
        }

        /// <summary>
        /// Get all results for an event (public)
        /// </summary>
        [AllowAnonymous]
        [HttpGet("event/{eventId}")]
        public async Task<IActionResult> GetEventResults(int eventId)
        {
            var eventItem = await _context.Events
                .FirstOrDefaultAsync(e => e.EventID == eventId);

            if (eventItem == null)
            {
                return NotFound(new { error = "Event not found." });
            }

            var results = await _context.Results
                .Include(r => r.Entry)
                    .ThenInclude(e => e!.Participant)
                .Where(r => r.Entry != null && r.Entry.EventID == eventId)
                .OrderBy(r => r.Position)
                .Select(r => new
                {
                    r.ResultID,
                    r.Position,
                    r.FinishTime,
                    r.Pace,
                    r.Status,
                    r.OverallPosition,
                    r.AgeGroupPosition,
                    r.GenderPosition,
                    ParticipantName = r.Entry != null && r.Entry.Participant != null
                        ? r.Entry.Participant.FullName : null,
                    BibNumber = r.Entry != null ? r.Entry.BibNumber : null
                })
                .ToListAsync();

            return Ok(new
            {
                EventID = eventId,
                EventName = eventItem.EventName,
                TotalResults = results.Count,
                Results = results
            });
        }

        /// <summary>
        /// Get leaderboard for an event (public)
        /// </summary>
        [AllowAnonymous]
        [HttpGet("leaderboard/{eventId}")]
        public async Task<IActionResult> GetLeaderboard(int eventId, [FromQuery] int limit = 10)
        {
            var eventItem = await _context.Events
                .FirstOrDefaultAsync(e => e.EventID == eventId);

            if (eventItem == null)
            {
                return NotFound(new { error = "Event not found." });
            }

            var leaderboard = await _context.Results
                .Include(r => r.Entry)
                    .ThenInclude(e => e!.Participant)
                .Where(r => r.Entry != null
                    && r.Entry.EventID == eventId
                    && r.Status == "Finished"
                    && r.Position != null)
                .OrderBy(r => r.Position)
                .Take(limit)
                .Select(r => new
                {
                    r.Position,
                    ParticipantName = r.Entry != null && r.Entry.Participant != null
                        ? r.Entry.Participant.FullName : null,
                    r.FinishTime,
                    r.Pace,
                    BibNumber = r.Entry != null ? r.Entry.BibNumber : null
                })
                .ToListAsync();

            return Ok(new
            {
                EventID = eventId,
                EventName = eventItem.EventName,
                Leaderboard = leaderboard
            });
        }

        /// <summary>
        /// Get result for a specific entry
        /// </summary>
        [HttpGet("entry/{entryId}")]
        [Authorize]
        public async Task<IActionResult> GetResultByEntry(int entryId)
        {
            var currentUserId = GetCurrentUserId();
            var currentRole = GetCurrentUserRole();

            var result = await _context.Results
                .Include(r => r.Entry)
                    .ThenInclude(e => e!.Participant)
                .Include(r => r.Entry)
                    .ThenInclude(e => e!.Event)
                .FirstOrDefaultAsync(r => r.EntryID == entryId);

            if (result == null)
            {
                return NotFound(new { error = "Result not found for this entry." });
            }

            if (currentRole != "Organiser" && result.Entry?.Participant?.UserID != currentUserId)
            {
                return Forbid();
            }

            return Ok(new
            {
                result.ResultID,
                result.FinishTime,
                result.Position,
                result.Pace,
                result.Status,
                result.CertificateURL,
                EventName = result.Entry?.Event?.EventName,
                ParticipantName = result.Entry?.Participant?.FullName
            });
        }

        /// <summary>
        /// Create a result (Organiser only, must own the event)
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> CreateResult([FromBody] Result newResult)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var currentUserId = GetCurrentUserId();

            // Find the entry
            var entry = await _context.Entries
                .Include(e => e.Event)
                    .ThenInclude(e => e!.Organiser)
                .FirstOrDefaultAsync(e => e.EntryID == newResult.EntryID);

            if (entry == null)
            {
                return NotFound(new { error = "Entry not found." });
            }

            // Verify organiser owns the event
            if (entry.Event?.Organiser == null || entry.Event.Organiser.UserID != currentUserId)
            {
                return Forbid();
            }

            // Check for existing result
            var existingResult = await _context.Results
                .AnyAsync(r => r.EntryID == newResult.EntryID);

            if (existingResult)
            {
                return BadRequest(new { error = "A result already exists for this entry." });
            }

            newResult.CreatedAt = DateTime.UtcNow;

            _context.Results.Add(newResult);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetResult), new { id = newResult.ResultID }, new
            {
                message = "Result created successfully.",
                newResult.ResultID
            });
        }

        /// <summary>
        /// Update a result (Organiser only)
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> UpdateResult(int id, [FromBody] Result updatedResult)
        {
            var currentUserId = GetCurrentUserId();

            var result = await _context.Results
                .Include(r => r.Entry)
                    .ThenInclude(e => e!.Event)
                        .ThenInclude(ev => ev!.Organiser)
                .FirstOrDefaultAsync(r => r.ResultID == id);

            if (result == null)
            {
                return NotFound(new { error = "Result not found." });
            }

            if (result.Entry?.Event?.Organiser == null
                || result.Entry.Event.Organiser.UserID != currentUserId)
            {
                return Forbid();
            }

            result.FinishTime = updatedResult.FinishTime ?? result.FinishTime;
            result.Position = updatedResult.Position ?? result.Position;
            result.GunTime = updatedResult.GunTime ?? result.GunTime;
            result.ChipTime = updatedResult.ChipTime ?? result.ChipTime;
            result.Pace = updatedResult.Pace ?? result.Pace;
            result.Status = updatedResult.Status ?? result.Status;
            result.OverallPosition = updatedResult.OverallPosition ?? result.OverallPosition;
            result.AgeGroupPosition = updatedResult.AgeGroupPosition ?? result.AgeGroupPosition;
            result.GenderPosition = updatedResult.GenderPosition ?? result.GenderPosition;
            result.CertificateURL = updatedResult.CertificateURL ?? result.CertificateURL;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Result updated successfully.",
                result.ResultID
            });
        }

        // Helper methods
        private int GetCurrentUserId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(claim, out var id) ? id : 0;
        }

        private string GetCurrentUserRole()
        {
            return User.FindFirst(ClaimTypes.Role)?.Value ?? string.Empty;
        }
    }
}
