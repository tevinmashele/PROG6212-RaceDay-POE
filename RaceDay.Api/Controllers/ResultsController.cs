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
    public class ResultsController : ControllerBase
    {
        private readonly RaceDayDbContext _context;

        public ResultsController(RaceDayDbContext context)
        {
            _context = context;
        }

        // POST: api/results (Organiser only, own events)
        [HttpPost("results")]
        [RoleAuthorize("Organiser")]
        public async Task<IActionResult> CaptureResult([FromBody] CreateResultRequest request)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return Unauthorized();

            var enrolment = await _context.Enrolments
                .Include(e => e.Event)
                .Include(e => e.Participant)
                .Include(e => e.Category)
                .FirstOrDefaultAsync(e => e.EnrolmentId == request.EnrolmentId);

            if (enrolment == null) return NotFound(new { message = "Enrolment not found" });

            if (enrolment.Event == null || enrolment.Event.OrganiserId != userId)
            {
                return StatusCode(403, new { message = "You can only capture results for your own events" });
            }

            // Check if result already exists
            var existingResult = await _context.Results
                .FirstOrDefaultAsync(r => r.EnrolmentId == request.EnrolmentId);

            if (existingResult != null)
            {
                return Conflict(new { message = "Result already captured for this enrolment" });
            }

            var result = new Result
            {
                EnrolmentId = request.EnrolmentId,
                FinishTime = request.FinishTime,
                FinishPosition = request.FinishPosition,
                CreatedDate = DateTime.UtcNow
            };

            _context.Results.Add(result);
            await _context.SaveChangesAsync();

            return StatusCode(201, new ResultDto
            {
                ResultId = result.ResultId,
                EnrolmentId = result.EnrolmentId,
                ParticipantName = enrolment.Participant != null
                    ? $"{enrolment.Participant.FirstName} {enrolment.Participant.LastName}"
                    : "",
                EventName = enrolment.Event.Name,
                FinishTime = result.FinishTime,
                FinishPosition = result.FinishPosition
            });
        }

        // GET: api/results/mine (Participant only)
        [HttpGet("results/mine")]
        [RoleAuthorize("Participant")]
        public async Task<IActionResult> GetMyResults()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return Unauthorized();

            var results = await _context.Results
                .Include(r => r.Enrolment)
                    .ThenInclude(e => e!.Event)
                .Include(r => r.Enrolment)
                    .ThenInclude(e => e!.Participant)
                .Where(r => r.Enrolment != null && r.Enrolment.ParticipantId == userId)
                .Select(r => new ResultDto
                {
                    ResultId = r.ResultId,
                    EnrolmentId = r.EnrolmentId,
                    ParticipantName = r.Enrolment != null && r.Enrolment.Participant != null
                        ? $"{r.Enrolment.Participant.FirstName} {r.Enrolment.Participant.LastName}"
                        : "",
                    EventName = r.Enrolment != null && r.Enrolment.Event != null
                        ? r.Enrolment.Event.Name
                        : "",
                    FinishTime = r.FinishTime,
                    FinishPosition = r.FinishPosition
                })
                .ToListAsync();

            return Ok(results);
        }

        // GET: api/results/event/{eventId} (Organiser only, own events)
        [HttpGet("results/event/{eventId}")]
        [RoleAuthorize("Organiser")]
        public async Task<IActionResult> GetResultsForEvent(int eventId)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return Unauthorized();

            var ev = await _context.Events.FindAsync(eventId);
            if (ev == null) return NotFound(new { message = "Event not found" });

            if (ev.OrganiserId != userId)
            {
                return StatusCode(403, new { message = "You can only view results for your own events" });
            }

            var results = await _context.Results
                .Include(r => r.Enrolment)
                    .ThenInclude(e => e!.Participant)
                .Where(r => r.Enrolment != null && r.Enrolment.EventId == eventId)
                .Select(r => new ResultDto
                {
                    ResultId = r.ResultId,
                    EnrolmentId = r.EnrolmentId,
                    ParticipantName = r.Enrolment != null && r.Enrolment.Participant != null
                        ? $"{r.Enrolment.Participant.FirstName} {r.Enrolment.Participant.LastName}"
                        : "",
                    EventName = ev.Name,
                    FinishTime = r.FinishTime,
                    FinishPosition = r.FinishPosition
                })
                .ToListAsync();

            return Ok(results);
        }
    }
}