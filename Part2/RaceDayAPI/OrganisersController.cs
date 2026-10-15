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
    public class OrganisersController : ControllerBase
    {
        private readonly RaceDayDbContext _context;

        public OrganisersController(RaceDayDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Get an organiser's profile by ID (public — anyone can view)
        /// </summary>
        [AllowAnonymous]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetOrganiser(int id)
        {
            var organiser = await _context.Organisers
                .Include(o => o.User)
                .FirstOrDefaultAsync(o => o.OrganiserID == id);

            if (organiser == null)
            {
                return NotFound(new { error = "Organiser not found." });
            }

            return Ok(new
            {
                organiser.OrganiserID,
                organiser.UserID,
                organiser.OrganisationName,
                organiser.RegistrationNumber,
                organiser.ContactPerson,
                organiser.PhoneNumber,
                organiser.Email,
                organiser.Website,
                organiser.IsVerified,
                organiser.CreatedAt
            });
        }

        /// <summary>
        /// Update organiser's own profile
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> UpdateOrganiser(int id, [FromBody] Organiser updatedOrganiser)
        {
            var currentUserId = GetCurrentUserId();

            var organiser = await _context.Organisers
                .FirstOrDefaultAsync(o => o.OrganiserID == id);

            if (organiser == null)
            {
                return NotFound(new { error = "Organiser not found." });
            }

            // Only the organiser themselves can update their profile
            if (organiser.UserID != currentUserId)
            {
                return Forbid();
            }

            // Update allowed fields
            organiser.OrganisationName = updatedOrganiser.OrganisationName ?? organiser.OrganisationName;
            organiser.ContactPerson = updatedOrganiser.ContactPerson ?? organiser.ContactPerson;
            organiser.PhoneNumber = updatedOrganiser.PhoneNumber ?? organiser.PhoneNumber;
            organiser.Email = updatedOrganiser.Email ?? organiser.Email;
            organiser.Website = updatedOrganiser.Website ?? organiser.Website;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Organiser profile updated successfully.",
                organiser.OrganiserID
            });
        }

        /// <summary>
        /// Get all events created by an organiser
        /// </summary>
        [AllowAnonymous]
        [HttpGet("{id}/events")]
        public async Task<IActionResult> GetOrganiserEvents(int id)
        {
            var organiser = await _context.Organisers
                .FirstOrDefaultAsync(o => o.OrganiserID == id);

            if (organiser == null)
            {
                return NotFound(new { error = "Organiser not found." });
            }

            var events = await _context.Events
                .Where(e => e.OrganiserID == id)
                .Select(e => new
                {
                    e.EventID,
                    e.EventName,
                    e.Description,
                    e.EventDate,
                    e.Venue,
                    e.City,
                    e.Province,
                    e.EventType,
                    e.Distance,
                    e.CurrentParticipants,
                    e.MaxParticipants,
                    e.EntryFee,
                    e.IsActive
                })
                .ToListAsync();

            return Ok(new
            {
                organiser.OrganiserID,
                organiser.OrganisationName,
                TotalEvents = events.Count,
                Events = events
            });
        }

        // Helper method
        private int GetCurrentUserId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(claim, out var id) ? id : 0;
        }
    }
}
