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
    public class EntriesController : ControllerBase
    {
        private readonly RaceDayDbContext _context;

        public EntriesController(RaceDayDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Get entry details by ID (Participant self or Organiser)
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetEntry(int id)
        {
            var currentUserId = GetCurrentUserId();
            var currentRole = GetCurrentUserRole();

            var entry = await _context.Entries
                .Include(e => e.Participant)
                .Include(e => e.Event)
                .Include(e => e.Category)
                .FirstOrDefaultAsync(e => e.EntryID == id);

            if (entry == null)
            {
                return NotFound(new { error = "Entry not found." });
            }

            // Only the participant themselves or an Organiser can view
            if (currentRole != "Organiser" && entry.Participant?.UserID != currentUserId)
            {
                return Forbid();
            }

            return Ok(new
            {
                entry.EntryID,
                entry.EntryDate,
                entry.Status,
                entry.BibNumber,
                entry.PaymentStatus,
                entry.PaymentDate,
                entry.EntryFeePaid,
                entry.TShirtSize,
                entry.DietaryRequirements,
                ParticipantName = entry.Participant?.FullName,
                EventName = entry.Event?.EventName,
                EventDate = entry.Event?.EventDate,
                CategoryName = entry.Category?.CategoryName
            });
        }

        /// <summary>
        /// Create a new entry (enrol for an event) - Participant only
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Participant")]
        public async Task<IActionResult> CreateEntry([FromBody] Entry newEntry)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var currentUserId = GetCurrentUserId();

            // Get the participant profile
            var participant = await _context.Participants
                .FirstOrDefaultAsync(p => p.UserID == currentUserId);

            if (participant == null)
            {
                return Forbid();
            }

            // Verify the event exists
            var eventItem = await _context.Events
                .FirstOrDefaultAsync(e => e.EventID == newEntry.EventID);

            if (eventItem == null)
            {
                return NotFound(new { error = "Event not found." });
            }

            // Check registration deadline
            if (eventItem.RegistrationDeadline < DateTime.UtcNow)
            {
                return BadRequest(new { error = "Registration deadline has passed." });
            }

            // Verify the category exists and belongs to the event
            var category = await _context.Categories
                .FirstOrDefaultAsync(c => c.CategoryID == newEntry.CategoryID);

            if (category == null || category.EventID != newEntry.EventID)
            {
                return BadRequest(new { error = "Invalid category for this event." });
            }

            // Check if the participant is already enrolled in this event
            var alreadyEnrolled = await _context.Entries
                .AnyAsync(e => e.ParticipantID == participant.ParticipantID
                    && e.EventID == newEntry.EventID
                    && e.Status != "Cancelled");

            if (alreadyEnrolled)
            {
                return BadRequest(new { error = "You are already enrolled in this event." });
            }

            // Check if the event is full
            if (eventItem.MaxParticipants.HasValue
                && eventItem.CurrentParticipants >= eventItem.MaxParticipants.Value)
            {
                return BadRequest(new { error = "Event is full." });
            }

            // Check if the category is full
            if (category.MaxParticipants.HasValue
                && category.CurrentParticipants >= category.MaxParticipants.Value)
            {
                return BadRequest(new { error = "Category is full." });
            }

            // Set values from token/context (ignore any provided)
            newEntry.ParticipantID = participant.ParticipantID;
            newEntry.EntryDate = DateTime.UtcNow;
            newEntry.Status = "Pending";
            newEntry.PaymentStatus = "Pending";
            newEntry.CreatedAt = DateTime.UtcNow;

            // Generate a bib number (simple - would be better with a sequence)
            var lastBib = await _context.Entries
                .Where(e => e.BibNumber != null)
                .MaxAsync(e => (int?)e.BibNumber) ?? 1000;
            newEntry.BibNumber = lastBib + 1;

            _context.Entries.Add(newEntry);

            // Update participant counts
            eventItem.CurrentParticipants++;
            category.CurrentParticipants++;

            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetEntry), new { id = newEntry.EntryID }, new
            {
                message = "Entry created successfully.",
                newEntry.EntryID,
                newEntry.BibNumber,
                newEntry.Status
            });
        }

        /// <summary>
        /// Update entry details (Participant can update own entry before confirmation)
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateEntry(int id, [FromBody] Entry updatedEntry)
        {
            var currentUserId = GetCurrentUserId();
            var currentRole = GetCurrentUserRole();

            var entry = await _context.Entries
                .Include(e => e.Participant)
                .FirstOrDefaultAsync(e => e.EntryID == id);

            if (entry == null)
            {
                return NotFound(new { error = "Entry not found." });
            }

            // Only participant owner or organiser
            if (currentRole != "Organiser" && entry.Participant?.UserID != currentUserId)
            {
                return Forbid();
            }

            // Cannot update if already confirmed
            if (entry.Status == "Confirmed" && currentRole != "Organiser")
            {
                return BadRequest(new { error = "Cannot update entry after confirmation." });
            }

            entry.TShirtSize = updatedEntry.TShirtSize ?? entry.TShirtSize;
            entry.DietaryRequirements = updatedEntry.DietaryRequirements ?? entry.DietaryRequirements;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Entry updated successfully.",
                entry.EntryID
            });
        }

        /// <summary>
        /// Cancel an entry (Participant self or Organiser)
        /// </summary>
        [HttpPut("{id}/cancel")]
        public async Task<IActionResult> CancelEntry(int id)
        {
            var currentUserId = GetCurrentUserId();
            var currentRole = GetCurrentUserRole();

            var entry = await _context.Entries
                .Include(e => e.Participant)
                .Include(e => e.Event)
                .Include(e => e.Category)
                .FirstOrDefaultAsync(e => e.EntryID == id);

            if (entry == null)
            {
                return NotFound(new { error = "Entry not found." });
            }

            if (currentRole != "Organiser" && entry.Participant?.UserID != currentUserId)
            {
                return Forbid();
            }

            if (entry.Status == "Cancelled")
            {
                return BadRequest(new { error = "Entry is already cancelled." });
            }

            if (entry.Event != null && entry.Event.EventDate < DateTime.UtcNow)
            {
                return BadRequest(new { error = "Cannot cancel entry after event has started." });
            }

            entry.Status = "Cancelled";

            // Decrement counters
            if (entry.Event != null) entry.Event.CurrentParticipants--;
            if (entry.Category != null) entry.Category.CurrentParticipants--;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Entry cancelled successfully.",
                entry.EntryID,
                entry.Status
            });
        }

        /// <summary>
        /// Get all entries for a specific event (Organiser only)
        /// </summary>
        [HttpGet("event/{eventId}")]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> GetEventEntries(int eventId)
        {
            var currentUserId = GetCurrentUserId();

            var eventItem = await _context.Events
                .Include(e => e.Organiser)
                .FirstOrDefaultAsync(e => e.EventID == eventId);

            if (eventItem == null)
            {
                return NotFound(new { error = "Event not found." });
            }

            if (eventItem.Organiser == null || eventItem.Organiser.UserID != currentUserId)
            {
                return Forbid();
            }

            var entries = await _context.Entries
                .Where(e => e.EventID == eventId)
                .Include(e => e.Participant)
                .Include(e => e.Category)
                .Select(e => new
                {
                    e.EntryID,
                    ParticipantName = e.Participant != null ? e.Participant.FullName : null,
                    CategoryName = e.Category != null ? e.Category.CategoryName : null,
                    e.Status,
                    e.BibNumber,
                    e.PaymentStatus,
                    e.EntryFeePaid,
                    e.TShirtSize,
                    e.EntryDate
                })
                .ToListAsync();

            return Ok(new
            {
                EventID = eventId,
                EventName = eventItem.EventName,
                TotalEntries = entries.Count,
                Entries = entries
            });
        }

        /// <summary>
        /// Update payment status (Participant self)
        /// </summary>
        [HttpPut("{id}/payment")]
        [Authorize(Roles = "Participant")]
        public async Task<IActionResult> UpdatePayment(int id, [FromBody] Entry paymentUpdate)
        {
            var currentUserId = GetCurrentUserId();

            var entry = await _context.Entries
                .Include(e => e.Participant)
                .FirstOrDefaultAsync(e => e.EntryID == id);

            if (entry == null)
            {
                return NotFound(new { error = "Entry not found." });
            }

            if (entry.Participant?.UserID != currentUserId)
            {
                return Forbid();
            }

            if (string.IsNullOrEmpty(paymentUpdate.PaymentReference))
            {
                return BadRequest(new { error = "Payment reference required." });
            }

            entry.PaymentReference = paymentUpdate.PaymentReference;
            entry.PaymentStatus = paymentUpdate.PaymentStatus ?? "Paid";
            entry.PaymentDate = DateTime.UtcNow;
            entry.EntryFeePaid = paymentUpdate.EntryFeePaid ?? entry.EntryFeePaid;
            entry.Status = "Confirmed";

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Payment updated successfully.",
                entry.EntryID,
                entry.PaymentStatus,
                entry.Status
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
