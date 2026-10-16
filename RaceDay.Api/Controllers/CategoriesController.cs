using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.Filters;
using RaceDay.Api.Models;
using RaceDay.Contracts;

namespace RaceDay.Api.Controllers
{
    [ApiController]
    [Route("api")]
    [RoleAuthorize]
    public class CategoriesController : ControllerBase
    {
        private readonly RaceDayDbContext _context;

        public CategoriesController(RaceDayDbContext context)
        {
            _context = context;
        }

        // GET: api/events/{eventId}/categories (Both roles)
        [HttpGet("events/{eventId}/categories")]
        public async Task<IActionResult> GetCategoriesForEvent(int eventId)
        {
            var eventExists = await _context.Events.AnyAsync(e => e.EventId == eventId);
            if (!eventExists) return NotFound(new { message = "Event not found" });

            var categories = await _context.Categories
                .Where(c => c.EventId == eventId)
                .Select(c => new CategoryDto
                {
                    CategoryId = c.CategoryId,
                    EventId = c.EventId,
                    Name = c.Name,
                    Description = c.Description,
                    EntryFee = c.EntryFee
                })
                .ToListAsync();

            return Ok(categories);
        }

        // POST: api/events/{eventId}/categories (Organiser only, own events)
        [HttpPost("events/{eventId}/categories")]
        [RoleAuthorize("Organiser")]
        public async Task<IActionResult> CreateCategory(int eventId, [FromBody] CreateCategoryRequest request)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return Unauthorized();

            var ev = await _context.Events.FindAsync(eventId);
            if (ev == null) return NotFound(new { message = "Event not found" });

            if (ev.OrganiserId != userId)
            {
                return StatusCode(403, new { message = "You can only add categories to your own events" });
            }

            var category = new Category
            {
                EventId = eventId,
                Name = request.Name,
                Description = request.Description,
                EntryFee = request.EntryFee
            };

            _context.Categories.Add(category);
            await _context.SaveChangesAsync();

            return StatusCode(201, new CategoryDto
            {
                CategoryId = category.CategoryId,
                EventId = category.EventId,
                Name = category.Name,
                Description = category.Description,
                EntryFee = category.EntryFee
            });
        }

        // PUT: api/categories/{id} (Organiser only, own events)
        [HttpPut("categories/{id}")]
        [RoleAuthorize("Organiser")]
        public async Task<IActionResult> UpdateCategory(int id, [FromBody] CreateCategoryRequest request)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return Unauthorized();

            var category = await _context.Categories
                .Include(c => c.Event)
                .FirstOrDefaultAsync(c => c.CategoryId == id);

            if (category == null) return NotFound(new { message = "Category not found" });

            if (category.Event == null || category.Event.OrganiserId != userId)
            {
                return StatusCode(403, new { message = "You can only edit categories of your own events" });
            }

            category.Name = request.Name;
            category.Description = request.Description;
            category.EntryFee = request.EntryFee;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/categories/{id} (Organiser only, own events)
        [HttpDelete("categories/{id}")]
        [RoleAuthorize("Organiser")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return Unauthorized();

            var category = await _context.Categories
                .Include(c => c.Event)
                .FirstOrDefaultAsync(c => c.CategoryId == id);

            if (category == null) return NotFound(new { message = "Category not found" });

            if (category.Event == null || category.Event.OrganiserId != userId)
            {
                return StatusCode(403, new { message = "You can only delete categories of your own events" });
            }

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}