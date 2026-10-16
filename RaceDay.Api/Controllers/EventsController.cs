using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.Filters;
using RaceDay.Api.Models;
using RaceDay.Contracts;

namespace RaceDay.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [RoleAuthorize]
    public class EventsController : ControllerBase
    {
        private readonly RaceDayDbContext _context;

        public EventsController(RaceDayDbContext context)
        {
            _context = context;
        }

        // GET: api/events (Both roles)
        [HttpGet]
        public async Task<IActionResult> GetAllEvents()
        {
            var events = await _context.Events
                .Include(e => e.Organiser)
                .Select(e => new EventDto
                {
                    EventId = e.EventId,
                    Name = e.Name,
                    Description = e.Description,
                    EventDate = e.EventDate,
                    Location = e.Location,
                    DistanceKm = e.DistanceKm,
                    EventType = e.EventType,
                    OrganiserId = e.OrganiserId,
                    OrganiserName = e.Organiser != null ? $"{e.Organiser.FirstName} {e.Organiser.LastName}" : ""
                })
                .ToListAsync();

            return Ok(events);
        }

        // GET: api/events/{id} (Both roles)
        [HttpGet("{id}")]
        public async Task<IActionResult> GetEventById(int id)
        {
            var e = await _context.Events
                .Include(ev => ev.Organiser)
                .FirstOrDefaultAsync(ev => ev.EventId == id);

            if (e == null) return NotFound(new { message = "Event not found" });

            return Ok(new EventDto
            {
                EventId = e.EventId,
                Name = e.Name,
                Description = e.Description,
                EventDate = e.EventDate,
                Location = e.Location,
                DistanceKm = e.DistanceKm,
                EventType = e.EventType,
                OrganiserId = e.OrganiserId,
                OrganiserName = e.Organiser != null ? $"{e.Organiser.FirstName} {e.Organiser.LastName}" : ""
            });
        }

        // GET: api/events/mine (Organiser only)
        [HttpGet("mine")]
        [RoleAuthorize("Organiser")]
        public async Task<IActionResult> GetMyEvents()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return Unauthorized();

            var events = await _context.Events
                .Where(e => e.OrganiserId == userId)
                .Select(e => new EventDto
                {
                    EventId = e.EventId,
                    Name = e.Name,
                    Description = e.Description,
                    EventDate = e.EventDate,
                    Location = e.Location,
                    DistanceKm = e.DistanceKm,
                    EventType = e.EventType,
                    OrganiserId = e.OrganiserId,
                    OrganiserName = ""
                })
                .ToListAsync();

            return Ok(events);
        }

        // POST: api/events (Organiser only)
        [HttpPost]
        [RoleAuthorize("Organiser")]
        public async Task<IActionResult> CreateEvent([FromBody] CreateEventRequest request)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return Unauthorized();

            var newEvent = new Event
            {
                Name = request.Name,
                Description = request.Description,
                EventDate = request.EventDate,
                Location = request.Location,
                DistanceKm = request.DistanceKm,
                EventType = request.EventType,
                OrganiserId = userId.Value,
                CreatedDate = DateTime.UtcNow
            };

            _context.Events.Add(newEvent);
            await _context.SaveChangesAsync();

            return StatusCode(201, new EventDto
            {
                EventId = newEvent.EventId,
                Name = newEvent.Name,
                Description = newEvent.Description,
                EventDate = newEvent.EventDate,
                Location = newEvent.Location,
                DistanceKm = newEvent.DistanceKm,
                EventType = newEvent.EventType,
                OrganiserId = newEvent.OrganiserId,
                OrganiserName = ""
            });
        }

        // PUT: api/events/{id} (Organiser only - own events)
        [HttpPut("{id}")]
        [RoleAuthorize("Organiser")]
        public async Task<IActionResult> UpdateEvent(int id, [FromBody] UpdateEventRequest request)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return Unauthorized();

            var existingEvent = await _context.Events.FindAsync(id);
            if (existingEvent == null) return NotFound(new { message = "Event not found" });

            if (existingEvent.OrganiserId != userId)
            {
                return StatusCode(403, new { message = "You can only edit your own events" });
            }

            existingEvent.Name = request.Name;
            existingEvent.Description = request.Description;
            existingEvent.EventDate = request.EventDate;
            existingEvent.Location = request.Location;
            existingEvent.DistanceKm = request.DistanceKm;
            existingEvent.EventType = request.EventType;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/events/{id} (Organiser only - own events)
        [HttpDelete("{id}")]
        [RoleAuthorize("Organiser")]
        public async Task<IActionResult> DeleteEvent(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return Unauthorized();

            var existingEvent = await _context.Events.FindAsync(id);
            if (existingEvent == null) return NotFound(new { message = "Event not found" });

            if (existingEvent.OrganiserId != userId)
            {
                return StatusCode(403, new { message = "You can only delete your own events" });
            }

            _context.Events.Remove(existingEvent);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}