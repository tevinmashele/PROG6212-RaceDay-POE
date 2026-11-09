using Microsoft.AspNetCore.Mvc;
using RaceDay.Contracts;
using RaceDay.Mvc.Services;
using RaceDay.Mvc.ViewModels;

namespace RaceDay.Mvc.Controllers
{
    public class AuthController : Controller
    {
        private readonly RaceDayApiClient _api;

        public AuthController(RaceDayApiClient api)
        {
            _api = api;
        }

        [HttpGet]
        public IActionResult Register() => View();

        [HttpPost]
        public async Task<IActionResult> Register(RegisterVm vm)
        {
            if (!ModelState.IsValid) return View(vm);

            try
            {
                var request = new RegisterRequest
                {
                    FirstName = vm.FirstName,
                    LastName = vm.LastName,
                    Email = vm.Email,
                    Password = vm.Password,
                    Role = vm.Role
                };

                var response = await _api.SendAsync(HttpMethod.Post, "api/auth/register", request);

                if (!response.IsSuccessStatusCode)
                {
                    string errorMsg = await response.Content.ReadAsStringAsync();
                    ModelState.AddModelError("", $"Registration failed: {errorMsg}");
                    return View(vm);
                }

                TempData["Success"] = "Registration successful. Please log in.";
                return RedirectToAction(nameof(Login));
            }
            catch (Exception)
            {
                ModelState.AddModelError("", "Could not connect to the API. Make sure the API is running.");
                return View(vm);
            }
        }

        [HttpGet]
        public IActionResult Login() => View();

        [HttpPost]
        public async Task<IActionResult> Login(LoginVm vm)
        {
            if (!ModelState.IsValid) return View(vm);

            try
            {
                var response = await _api.SendAsync(HttpMethod.Post, "api/auth/login",
                    new LoginRequest { Email = vm.Email, Password = vm.Password });

                if (!response.IsSuccessStatusCode)
                {
                    ModelState.AddModelError("", "Invalid email or password.");
                    return View(vm);
                }

                var auth = await _api.ReadAsync<AuthResponse>(response);
                if (auth == null)
                {
                    ModelState.AddModelError("", "Login response could not be read.");
                    return View(vm);
                }

                // Save API Cookie
                string? apiCookie = RaceDayApiClient.ExtractSessionCookie(response);
                if (!string.IsNullOrWhiteSpace(apiCookie))
                    HttpContext.Session.SetString("ApiSessionCookie", apiCookie);

                // Save MVC Session
                HttpContext.Session.SetInt32("UserId", auth.UserId);
                HttpContext.Session.SetString("Role", auth.Role);
                HttpContext.Session.SetString("DisplayName", auth.FirstName ?? auth.Email);

                if (auth.Role.Equals("Organiser", StringComparison.OrdinalIgnoreCase))
                    return RedirectToAction("Index", "Organiser");

                return RedirectToAction("Index", "Participant");
            }
            catch (Exception)
            {
                ModelState.AddModelError("", "Could not connect to the API. Make sure the API is running.");
                return View(vm);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            try
            {
                await _api.SendAsync(HttpMethod.Post, "api/auth/logout");
            }
            catch
            {
                // Ignore API errors on logout
            }

            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }
    }
}