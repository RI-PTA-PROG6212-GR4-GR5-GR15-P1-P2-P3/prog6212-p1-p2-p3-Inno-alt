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
    [Authorize]
    public class ParticipantsController : ControllerBase
    {
        private readonly RaceDayDbContext _context;

        public ParticipantsController(RaceDayDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Get a participant's profile by ID (only self or Organiser)
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetParticipant(int id)
        {
            var currentUserId = GetCurrentUserId();
            var currentRole = GetCurrentUserRole();

            var participant = await _context.Participants
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.ParticipantID == id);

            if (participant == null)
            {
                return NotFound(new { error = "Participant not found." });
            }

            // Only the participant themselves or an Organiser can view
            if (currentRole != "Organiser" && participant.UserID != currentUserId)
            {
                return Forbid();
            }

            return Ok(new
            {
                participant.ParticipantID,
                participant.UserID,
                participant.FullName,
                participant.IDNumber,
                participant.PhoneNumber,
                participant.DateOfBirth,
                participant.Gender,
                participant.EmergencyContactName,
                participant.EmergencyContactNumber,
                participant.MedicalConditions,
                Email = participant.User?.Email,
                participant.CreatedAt
            });
        }

        /// <summary>
        /// Update a participant's own profile
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateParticipant(int id, [FromBody] Participant updatedParticipant)
        {
            var currentUserId = GetCurrentUserId();
            var currentRole = GetCurrentUserRole();

            var participant = await _context.Participants
                .FirstOrDefaultAsync(p => p.ParticipantID == id);

            if (participant == null)
            {
                return NotFound(new { error = "Participant not found." });
            }

            // Only the participant themselves can update
            if (currentRole != "Organiser" && participant.UserID != currentUserId)
            {
                return Forbid();
            }

            // Update allowed fields
            participant.FullName = updatedParticipant.FullName ?? participant.FullName;
            participant.PhoneNumber = updatedParticipant.PhoneNumber ?? participant.PhoneNumber;
            participant.EmergencyContactName = updatedParticipant.EmergencyContactName ?? participant.EmergencyContactName;
            participant.EmergencyContactNumber = updatedParticipant.EmergencyContactNumber ?? participant.EmergencyContactNumber;
            participant.MedicalConditions = updatedParticipant.MedicalConditions ?? participant.MedicalConditions;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Participant profile updated successfully.",
                participant.ParticipantID
            });
        }

        /// <summary>
        /// Get all entries (enrolments) for a participant
        /// </summary>
        [HttpGet("{id}/entries")]
        public async Task<IActionResult> GetParticipantEntries(int id)
        {
            var currentUserId = GetCurrentUserId();
            var currentRole = GetCurrentUserRole();

            var participant = await _context.Participants
                .FirstOrDefaultAsync(p => p.ParticipantID == id);

            if (participant == null)
            {
                return NotFound(new { error = "Participant not found." });
            }

            if (currentRole != "Organiser" && participant.UserID != currentUserId)
            {
                return Forbid();
            }

            var entries = await _context.Entries
                .Where(e => e.ParticipantID == id)
                .Include(e => e.Event)
                .Include(e => e.Category)
                .Select(e => new
                {
                    e.EntryID,
                    e.Status,
                    e.BibNumber,
                    e.PaymentStatus,
                    e.EntryFeePaid,
                    EventName = e.Event != null ? e.Event.EventName : null,
                    EventDate = e.Event != null ? e.Event.EventDate : (DateTime?)null,
                    CategoryName = e.Category != null ? e.Category.CategoryName : null,
                    e.EntryDate
                })
                .ToListAsync();

            return Ok(new
            {
                participant.ParticipantID,
                participant.FullName,
                TotalEntries = entries.Count,
                Entries = entries
            });
        }

        /// <summary>
        /// Get all race results for a participant (performance history)
        /// </summary>
        [HttpGet("{id}/results")]
        public async Task<IActionResult> GetParticipantResults(int id)
        {
            var currentUserId = GetCurrentUserId();
            var currentRole = GetCurrentUserRole();

            var participant = await _context.Participants
                .FirstOrDefaultAsync(p => p.ParticipantID == id);

            if (participant == null)
            {
                return NotFound(new { error = "Participant not found." });
            }

            if (currentRole != "Organiser" && participant.UserID != currentUserId)
            {
                return Forbid();
            }

            var results = await _context.Results
                .Include(r => r.Entry)
                    .ThenInclude(e => e!.Event)
                .Where(r => r.Entry != null && r.Entry.ParticipantID == id)
                .Select(r => new
                {
                    r.ResultID,
                    r.FinishTime,
                    r.Position,
                    r.Pace,
                    r.Status,
                    r.OverallPosition,
                    r.AgeGroupPosition,
                    r.GenderPosition,
                    r.CertificateURL,
                    EventName = r.Entry!.Event != null ? r.Entry.Event.EventName : null,
                    EventDate = r.Entry.Event != null ? r.Entry.Event.EventDate : (DateTime?)null
                })
                .ToListAsync();

            return Ok(new
            {
                participant.ParticipantID,
                participant.FullName,
                TotalResults = results.Count,
                Results = results
            });
        }

        // Helper methods to get current user info from JWT
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