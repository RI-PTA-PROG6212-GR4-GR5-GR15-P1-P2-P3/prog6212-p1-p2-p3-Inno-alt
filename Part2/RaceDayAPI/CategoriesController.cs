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
    public class CategoriesController : ControllerBase
    {
        private readonly RaceDayDbContext _context;

        public CategoriesController(RaceDayDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Get all categories for a specific event (public)
        /// </summary>
        [AllowAnonymous]
        [HttpGet("event/{eventId}")]
        public async Task<IActionResult> GetEventCategories(int eventId)
        {
            var eventItem = await _context.Events
                .FirstOrDefaultAsync(e => e.EventID == eventId);

            if (eventItem == null)
            {
                return NotFound(new { error = "Event not found." });
            }

            var categories = await _context.Categories
                .Where(c => c.EventID == eventId)
                .Select(c => new
                {
                    c.CategoryID,
                    c.EventID,
                    c.CategoryName,
                    c.AgeGroupMin,
                    c.AgeGroupMax,
                    c.Distance,
                    c.GenderRestriction,
                    c.EntryFee,
                    c.MaxParticipants,
                    c.CurrentParticipants
                })
                .ToListAsync();

            return Ok(new
            {
                EventID = eventId,
                EventName = eventItem.EventName,
                Total = categories.Count,
                Categories = categories
            });
        }

        /// <summary>
        /// Get a specific category by ID (public)
        /// </summary>
        [AllowAnonymous]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetCategory(int id)
        {
            var category = await _context.Categories
                .Include(c => c.Event)
                .FirstOrDefaultAsync(c => c.CategoryID == id);

            if (category == null)
            {
                return NotFound(new { error = "Category not found." });
            }

            return Ok(new
            {
                category.CategoryID,
                category.CategoryName,
                category.AgeGroupMin,
                category.AgeGroupMax,
                category.Distance,
                category.GenderRestriction,
                category.EntryFee,
                category.MaxParticipants,
                category.CurrentParticipants,
                EventID = category.EventID,
                EventName = category.Event?.EventName
            });
        }

        /// <summary>
        /// Create a new category for an event (Organiser only, must own the event)
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> CreateCategory([FromBody] Category newCategory)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var currentUserId = GetCurrentUserId();

            // Verify the organiser owns the event
            var eventItem = await _context.Events
                .Include(e => e.Organiser)
                .FirstOrDefaultAsync(e => e.EventID == newCategory.EventID);

            if (eventItem == null)
            {
                return NotFound(new { error = "Event not found." });
            }

            if (eventItem.Organiser == null || eventItem.Organiser.UserID != currentUserId)
            {
                return Forbid();
            }

            // Check if category name already exists for this event
            var duplicate = await _context.Categories
                .AnyAsync(c => c.EventID == newCategory.EventID
                    && c.CategoryName == newCategory.CategoryName);

            if (duplicate)
            {
                return BadRequest(new { error = "A category with this name already exists for this event." });
            }

            newCategory.CreatedAt = DateTime.UtcNow;
            newCategory.CurrentParticipants = 0;

            _context.Categories.Add(newCategory);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetCategory), new { id = newCategory.CategoryID }, new
            {
                message = "Category created successfully.",
                newCategory.CategoryID
            });
        }

        /// <summary>
        /// Update a category (Organiser only, must own the event)
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> UpdateCategory(int id, [FromBody] Category updatedCategory)
        {
            var currentUserId = GetCurrentUserId();

            var category = await _context.Categories
                .Include(c => c.Event)
                    .ThenInclude(e => e!.Organiser)
                .FirstOrDefaultAsync(c => c.CategoryID == id);

            if (category == null)
            {
                return NotFound(new { error = "Category not found." });
            }

            if (category.Event?.Organiser == null || category.Event.Organiser.UserID != currentUserId)
            {
                return Forbid();
            }

            category.CategoryName = updatedCategory.CategoryName ?? category.CategoryName;
            category.AgeGroupMin = updatedCategory.AgeGroupMin ?? category.AgeGroupMin;
            category.AgeGroupMax = updatedCategory.AgeGroupMax ?? category.AgeGroupMax;
            category.Distance = updatedCategory.Distance ?? category.Distance;
            category.GenderRestriction = updatedCategory.GenderRestriction ?? category.GenderRestriction;
            category.EntryFee = updatedCategory.EntryFee ?? category.EntryFee;
            category.MaxParticipants = updatedCategory.MaxParticipants ?? category.MaxParticipants;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Category updated successfully.",
                category.CategoryID
            });
        }

        /// <summary>
        /// Delete a category (Organiser only, must own the event, no active entries)
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var currentUserId = GetCurrentUserId();

            var category = await _context.Categories
                .Include(c => c.Event)
                    .ThenInclude(e => e!.Organiser)
                .Include(c => c.Entries)
                .FirstOrDefaultAsync(c => c.CategoryID == id);

            if (category == null)
            {
                return NotFound(new { error = "Category not found." });
            }

            if (category.Event?.Organiser == null || category.Event.Organiser.UserID != currentUserId)
            {
                return Forbid();
            }

            // Prevent deletion if there are active entries
            if (category.Entries.Any(e => e.Status != "Cancelled"))
            {
                return BadRequest(new { error = "Cannot delete a category with active entries." });
            }

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Category deleted successfully." });
        }

        // Helper method
        private int GetCurrentUserId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(claim, out var id) ? id : 0;
        }
    }
}
