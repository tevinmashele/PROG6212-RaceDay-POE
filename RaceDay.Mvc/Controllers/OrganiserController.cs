using Microsoft.AspNetCore.Mvc;
using RaceDay.Contracts;
using RaceDay.Mvc.Services;

namespace RaceDay.Mvc.Controllers
{
    public class OrganiserController : Controller
    {
        private readonly RaceDayApiClient _api;

        public OrganiserController(RaceDayApiClient api)
        {
            _api = api;
        }

        private bool IsOrganiser()
        {
            return HttpContext.Session.GetString("Role") == "Organiser";
        }

        public IActionResult Index()
        {
            if (!IsOrganiser()) return RedirectToAction("Login", "Auth");
            return View();
        }

        public async Task<IActionResult> Events()
        {
            if (!IsOrganiser()) return RedirectToAction("Login", "Auth");
            var response = await _api.SendAsync(HttpMethod.Get, "api/events/mine");
            var events = await _api.ReadAsync<List<EventDto>>(response);
            return View(events ?? new List<EventDto>());
        }

        [HttpGet]
        public IActionResult CreateEvent()
        {
            if (!IsOrganiser()) return RedirectToAction("Login", "Auth");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateEvent(CreateEventRequest request)
        {
            if (!IsOrganiser()) return RedirectToAction("Login", "Auth");
            if (!ModelState.IsValid) return View(request);
            var response = await _api.SendAsync(HttpMethod.Post, "api/events", request);
            if (!response.IsSuccessStatusCode)
            {
                string error = await response.Content.ReadAsStringAsync();
                ModelState.AddModelError("", $"Failed: {error}");
                return View(request);
            }
            TempData["Success"] = "Event created successfully!";
            return RedirectToAction(nameof(Events));
        }

        [HttpGet]
        public async Task<IActionResult> ManageCategories(int eventId)
        {
            if (!IsOrganiser()) return RedirectToAction("Login", "Auth");
            var evResponse = await _api.SendAsync(HttpMethod.Get, $"api/events/{eventId}");
            var ev = await _api.ReadAsync<EventDto>(evResponse);
            var catResponse = await _api.SendAsync(HttpMethod.Get, $"api/events/{eventId}/categories");
            var categories = await _api.ReadAsync<List<CategoryDto>>(catResponse) ?? new List<CategoryDto>();
            ViewBag.Event = ev;
            return View(categories);
        }

        [HttpPost]
        public async Task<IActionResult> AddCategory(int eventId, CreateCategoryRequest request)
        {
            if (!IsOrganiser()) return RedirectToAction("Login", "Auth");
            var response = await _api.SendAsync(HttpMethod.Post, $"api/events/{eventId}/categories", request);
            if (!response.IsSuccessStatusCode) TempData["Error"] = "Failed to add category.";
            else TempData["Success"] = "Category added successfully!";
            return RedirectToAction(nameof(ManageCategories), new { eventId = eventId });
        }

        // --- NEW METHODS FOR ENROLMENTS AND RESULTS ---

        // GET: /Organiser/Enrolments/5
        [HttpGet]
        public async Task<IActionResult> Enrolments(int eventId)
        {
            if (!IsOrganiser()) return RedirectToAction("Login", "Auth");

            var response = await _api.SendAsync(HttpMethod.Get, $"api/enrolments/event/{eventId}");
            var enrolments = await _api.ReadAsync<List<EnrolmentDto>>(response) ?? new List<EnrolmentDto>();

            var evResponse = await _api.SendAsync(HttpMethod.Get, $"api/events/{eventId}");
            var ev = await _api.ReadAsync<EventDto>(evResponse);

            ViewBag.Event = ev;
            return View(enrolments);
        }

        // POST: /Organiser/CaptureResult
        [HttpPost]
        public async Task<IActionResult> CaptureResult(CreateResultRequest request, int eventId)
        {
            if (!IsOrganiser()) return RedirectToAction("Login", "Auth");

            var response = await _api.SendAsync(HttpMethod.Post, "api/results", request);

            if (!response.IsSuccessStatusCode)
            {
                string error = await response.Content.ReadAsStringAsync();
                TempData["Error"] = $"Failed to capture result: {error}";
            }
            else
            {
                TempData["Success"] = "Result captured successfully!";
            }

            return RedirectToAction(nameof(Enrolments), new { eventId = eventId });
        }
    }
}