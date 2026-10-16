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
    public class EnrolmentsController : ControllerBase
    {
        private readonly RaceDayDbContext _context;

        public EnrolmentsController(RaceDayDbContext context)
        {
            _context = context;
        }

        // POST: api/enrolments (Participant only)
        [HttpPost]
        [RoleAuthorize("Participant")]
        public async Task<IActionResult> CreateEnrolment([FromBody] CreateEnrolmentRequest request)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return Unauthorized();

            // Check event exists
            var ev = await _context.Events.FindAsync(request.EventId);
            if (ev == null) return NotFound(new { message = "Event not found" });

            // Check category exists and belongs to event
            var category = await _context.Categories.FindAsync(request.CategoryId);
            if (category == null) return NotFound(new { message = "Category not found" });

            if (category.EventId != request.EventId)
            {
                return BadRequest(new { message = "Category does not belong to this event" });
            }

            // Check for duplicate enrolment
            var existing = await _context.Enrolments
                .FirstOrDefaultAsync(e => e.ParticipantId == userId && e.EventId == request.EventId);

            if (existing != null)
            {
                return Conflict(new { message = "You are already enrolled in this event" });
            }

            var enrolment = new Enrolment
            {
                ParticipantId = userId.Value,
                EventId = request.EventId,
                CategoryId = request.CategoryId,
                EnrolmentDate = DateTime.UtcNow,
                Status = "Confirmed"
            };

            _context.Enrolments.Add(enrolment);
            await _context.SaveChangesAsync();

            return StatusCode(201, new EnrolmentDto
            {
                EnrolmentId = enrolment.EnrolmentId,
                ParticipantId = enrolment.ParticipantId,
                ParticipantName = "",
                EventId = enrolment.EventId,
                EventName = ev.Name,
                CategoryId = enrolment.CategoryId,
                CategoryName = category.Name,
                EnrolmentDate = enrolment.EnrolmentDate,
                Status = enrolment.Status
            });
        }

        // GET: api/enrolments/mine (Participant only)
        [HttpGet("mine")]
        [RoleAuthorize("Participant")]
        public async Task<IActionResult> GetMyEnrolments()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return Unauthorized();

            var enrolments = await _context.Enrolments
                .Include(e => e.Event)
                .Include(e => e.Category)
                .Where(e => e.ParticipantId == userId)
                .Select(e => new EnrolmentDto
                {
                    EnrolmentId = e.EnrolmentId,
                    ParticipantId = e.ParticipantId,
                    ParticipantName = "",
                    EventId = e.EventId,
                    EventName = e.Event != null ? e.Event.Name : "",
                    CategoryId = e.CategoryId,
                    CategoryName = e.Category != null ? e.Category.Name : "",
                    EnrolmentDate = e.EnrolmentDate,
                    Status = e.Status
                })
                .ToListAsync();

            return Ok(enrolments);
        }

        // GET: api/enrolments/event/{eventId} (Organiser only, own events)
        [HttpGet("event/{eventId}")]
        [RoleAuthorize("Organiser")]
        public async Task<IActionResult> GetEnrolmentsForEvent(int eventId)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return Unauthorized();

            var ev = await _context.Events.FindAsync(eventId);
            if (ev == null) return NotFound(new { message = "Event not found" });

            if (ev.OrganiserId != userId)
            {
                return StatusCode(403, new { message = "You can only view enrolments for your own events" });
            }

            var enrolments = await _context.Enrolments
                .Include(e => e.Participant)
                .Include(e => e.Category)
                .Where(e => e.EventId == eventId)
                .Select(e => new EnrolmentDto
                {
                    EnrolmentId = e.EnrolmentId,
                    ParticipantId = e.ParticipantId,
                    ParticipantName = e.Participant != null ? $"{e.Participant.FirstName} {e.Participant.LastName}" : "",
                    EventId = e.EventId,
                    EventName = ev.Name,
                    CategoryId = e.CategoryId,
                    CategoryName = e.Category != null ? e.Category.Name : "",
                    EnrolmentDate = e.EnrolmentDate,
                    Status = e.Status
                })
                .ToListAsync();

            return Ok(enrolments);
        }

        // DELETE: api/enrolments/{id} (Participant only - own enrolment)
        [HttpDelete("{id}")]
        [RoleAuthorize("Participant")]
        public async Task<IActionResult> WithdrawFromEvent(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return Unauthorized();

            var enrolment = await _context.Enrolments.FindAsync(id);
            if (enrolment == null) return NotFound(new { message = "Enrolment not found" });

            if (enrolment.ParticipantId != userId)
            {
                return StatusCode(403, new { message = "You can only withdraw from your own enrolments" });
            }

            _context.Enrolments.Remove(enrolment);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}