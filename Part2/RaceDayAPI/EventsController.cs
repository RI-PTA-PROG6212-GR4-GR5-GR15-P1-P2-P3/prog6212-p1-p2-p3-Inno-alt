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
    public class EventsController : ControllerBase
    {
        private readonly RaceDayDbContext _context;

        public EventsController(RaceDayDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Get all events (public — anyone can view)
        /// </summary>
        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> GetAllEvents(
            [FromQuery] string? city,
            [FromQuery] string? province,
            [FromQuery] string? eventType,
            [FromQuery] string? search)
        {
            var query = _context.Events
                .Include(e => e.Organiser)
                .Where(e => e.IsActive)
                .AsQueryable();

            if (!string.IsNullOrEmpty(city))
                query = query.Where(e => e.City.Contains(city));

            if (!string.IsNullOrEmpty(province))
                query = query.Where(e => e.Province.Contains(province));

            if (!string.IsNullOrEmpty(eventType))
                query = query.Where(e => e.EventType == eventType);

            if (!string.IsNullOrEmpty(search))
                query = query.Where(e => e.EventName.Contains(search));

            var events = await query
                .Select(e => new
                {
                    e.EventID,
                    e.EventName,
                    e.Description,
                    e.EventDate,
                    e.RegistrationDeadline,
                    e.Venue,
                    e.City,
                    e.Province,
                    e.EventType,
                    e.Distance,
                    e.MaxParticipants,
                    e.CurrentParticipants,
                    e.EntryFee,
                    e.IsActive,
                    OrganiserName = e.Organiser != null ? e.Organiser.OrganisationName : null
                })
                .ToListAsync();

            return Ok(new
            {
                Total = events.Count,
                Events = events
            });
        }

        /// <summary>
        /// Get upcoming events (public)
        /// </summary>
        [AllowAnonymous]
        [HttpGet("upcoming")]
        public async Task<IActionResult> GetUpcomingEvents()
        {
            var upcoming = await _context.Events
                .Where(e => e.EventDate > DateTime.UtcNow && e.IsActive)
                .OrderBy(e => e.EventDate)
                .Select(e => new
                {
                    e.EventID,
                    e.EventName,
                    e.EventDate,
                    e.Venue,
                    e.City,
                    e.Province,
                    e.EventType,
                    e.Distance,
                    e.EntryFee
                })
                .ToListAsync();

            return Ok(new
            {
                Total = upcoming.Count,
                UpcomingEvents = upcoming
            });
        }

        /// <summary>
        /// Get a specific event by ID (public)
        /// </summary>
        [AllowAnonymous]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetEvent(int id)
        {
            var eventItem = await _context.Events
                .Include(e => e.Organiser)
                .Include(e => e.Categories)
                .FirstOrDefaultAsync(e => e.EventID == id);

            if (eventItem == null)
            {
                return NotFound(new { error = "Event not found." });
            }

            return Ok(new
            {
                eventItem.EventID,
                eventItem.EventName,
                eventItem.Description,
                eventItem.EventDate,
                eventItem.RegistrationDeadline,
                eventItem.Venue,
                eventItem.City,
                eventItem.Province,
                eventItem.EventType,
                eventItem.Distance,
                eventItem.MaxParticipants,
                eventItem.CurrentParticipants,
                eventItem.EntryFee,
                eventItem.IsActive,
                OrganiserName = eventItem.Organiser?.OrganisationName,
                OrganiserContact = eventItem.Organiser?.ContactPerson,
                OrganiserEmail = eventItem.Organiser?.Email,
                Categories = eventItem.Categories.Select(c => new
                {
                    c.CategoryID,
                    c.CategoryName,
                    c.Distance,
                    c.EntryFee,
                    c.MaxParticipants,
                    c.CurrentParticipants
                })
            });
        }

        /// <summary>
        /// Create a new event (Organiser only)
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> CreateEvent([FromBody] Event newEvent)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var currentUserId = GetCurrentUserId();

            // Get the organiser profile
            var organiser = await _context.Organisers
                .FirstOrDefaultAsync(o => o.UserID == currentUserId);

            if (organiser == null)
            {
                return Forbid();
            }

            // Validation: Registration deadline must be before event date
            if (newEvent.RegistrationDeadline >= newEvent.EventDate)
            {
                return BadRequest(new { error = "Registration deadline must be before the event date." });
            }

            if (newEvent.EventDate < DateTime.UtcNow)
            {
                return BadRequest(new { error = "Event date cannot be in the past." });
            }

            // Set organiser ID from token (ignore any provided value)
            newEvent.OrganiserID = organiser.OrganiserID;
            newEvent.CreatedAt = DateTime.UtcNow;
            newEvent.CurrentParticipants = 0;
            newEvent.IsActive = true;

            _context.Events.Add(newEvent);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetEvent), new { id = newEvent.EventID }, new
            {
                message = "Event created successfully.",
                newEvent.EventID
            });
        }

        /// <summary>
        /// Update an event (only the creator organiser)
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> UpdateEvent(int id, [FromBody] Event updatedEvent)
        {
            var currentUserId = GetCurrentUserId();

            var eventItem = await _context.Events
                .Include(e => e.Organiser)
                .FirstOrDefaultAsync(e => e.EventID == id);

            if (eventItem == null)
            {
                return NotFound(new { error = "Event not found." });
            }

            // Check if current user owns this event
            if (eventItem.Organiser == null || eventItem.Organiser.UserID != currentUserId)
            {
                return Forbid();
            }

            eventItem.EventName = updatedEvent.EventName ?? eventItem.EventName;
            eventItem.Description = updatedEvent.Description ?? eventItem.Description;
            eventItem.Venue = updatedEvent.Venue ?? eventItem.Venue;
            eventItem.City = updatedEvent.City ?? eventItem.City;
            eventItem.Province = updatedEvent.Province ?? eventItem.Province;
            eventItem.EventType = updatedEvent.EventType ?? eventItem.EventType;
            eventItem.Distance = updatedEvent.Distance ?? eventItem.Distance;
            eventItem.MaxParticipants = updatedEvent.MaxParticipants ?? eventItem.MaxParticipants;
            eventItem.EntryFee = updatedEvent.EntryFee ?? eventItem.EntryFee;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Event updated successfully.",
                eventItem.EventID
            });
        }

        /// <summary>
        /// Delete an event (only the creator organiser)
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> DeleteEvent(int id)
        {
            var currentUserId = GetCurrentUserId();

            var eventItem = await _context.Events
                .Include(e => e.Organiser)
                .FirstOrDefaultAsync(e => e.EventID == id);

            if (eventItem == null)
            {
                return NotFound(new { error = "Event not found." });
            }

            if (eventItem.Organiser == null || eventItem.Organiser.UserID != currentUserId)
            {
                return Forbid();
            }

            // Soft delete
            eventItem.IsActive = false;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Event deleted successfully." });
        }

        // Helper method
        private int GetCurrentUserId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(claim, out var id) ? id : 0;
        }
    }
}
