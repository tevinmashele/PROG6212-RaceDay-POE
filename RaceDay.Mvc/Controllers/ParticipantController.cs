using Microsoft.AspNetCore.Mvc;
using RaceDay.Contracts;
using RaceDay.Mvc.Services;

namespace RaceDay.Mvc.Controllers
{
    public class ParticipantController : Controller
    {
        private readonly RaceDayApiClient _api;

        public ParticipantController(RaceDayApiClient api)
        {
            _api = api;
        }

        private bool IsParticipant()
        {
            return HttpContext.Session.GetString("Role") == "Participant";
        }

        public IActionResult Index()
        {
            if (!IsParticipant()) return RedirectToAction("Login", "Auth");
            return View();
        }

        public async Task<IActionResult> BrowseEvents()
        {
            if (!IsParticipant()) return RedirectToAction("Login", "Auth");
            var response = await _api.SendAsync(HttpMethod.Get, "api/events");
            var events = await _api.ReadAsync<List<EventDto>>(response);
            return View(events ?? new List<EventDto>());
        }

        [HttpGet]
        public async Task<IActionResult> EnterEvent(int id)
        {
            if (!IsParticipant()) return RedirectToAction("Login", "Auth");
            var response = await _api.SendAsync(HttpMethod.Get, $"api/events/{id}");
            var ev = await _api.ReadAsync<EventDto>(response);
            if (ev == null) return NotFound();

            var catResponse = await _api.SendAsync(HttpMethod.Get, $"api/events/{id}/categories");
            var categories = await _api.ReadAsync<List<CategoryDto>>(catResponse) ?? new List<CategoryDto>();

            ViewBag.Event = ev;
            return View(categories);
        }

        [HttpPost]
        public async Task<IActionResult> EnterEvent(int eventId, int categoryId)
        {
            if (!IsParticipant()) return RedirectToAction("Login", "Auth");
            var request = new CreateEnrolmentRequest { EventId = eventId, CategoryId = categoryId };
            var response = await _api.SendAsync(HttpMethod.Post, "api/enrolments", request);

            if (!response.IsSuccessStatusCode)
            {
                string error = await response.Content.ReadAsStringAsync();
                TempData["Error"] = $"Failed to enrol: {error}";
                return RedirectToAction(nameof(BrowseEvents));
            }

            TempData["Success"] = "You have successfully entered the event!";
            return RedirectToAction(nameof(MyEvents));
        }

        public async Task<IActionResult> MyEvents()
        {
            if (!IsParticipant()) return RedirectToAction("Login", "Auth");
            var response = await _api.SendAsync(HttpMethod.Get, "api/enrolments/mine");
            var enrolments = await _api.ReadAsync<List<EnrolmentDto>>(response);
            return View(enrolments ?? new List<EnrolmentDto>());
        }

        // NEW METHOD FOR RESULTS
        public async Task<IActionResult> MyResults()
        {
            if (!IsParticipant()) return RedirectToAction("Login", "Auth");
            var response = await _api.SendAsync(HttpMethod.Get, "api/results/mine");
            var results = await _api.ReadAsync<List<ResultDto>>(response);
            return View(results ?? new List<ResultDto>());
        }
    }
}