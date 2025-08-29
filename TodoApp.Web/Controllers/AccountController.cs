// Controllers/AccountController.cs
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using TodoApp.Web.Models;
using TodoApp.Web.Services;

namespace TodoApp.Web.Controllers
{
    public class AccountController : Controller
    {
        private readonly IApiService _apiService;
        private readonly IConfiguration _configuration;

        public AccountController(IApiService apiService, IConfiguration configuration)
        {
            _apiService = apiService;
            _configuration = configuration;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var response = await _apiService.PostAsync<AuthResponse>("api/auth/login", new
            {
                model.Email,
                model.Password
            });

            if (response.Success)
            {
                await SignInUser(response.Data.Token, model.RememberMe);
                return RedirectToAction("Dashboard", "Home");
            }

            ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            return View(model);
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = User;

            var roles = User.Claims
             .Where(c => c.Type == ClaimTypes.Role)
             .Select(c => c.Value)
             .ToList();

            var response = await _apiService.PostAsync<AuthResponse>("api/auth/register", new
            {
                model.FirstName,
                model.LastName,
                model.Email,
                model.Password,
                model.ConfirmPassword
            });

            if (response.Success)
            {
                if (roles.Contains("Admin") || roles.Contains("admin"))
                {
                    return RedirectToAction("Index", "Users");
                }
                await SignInUser(response.Data.Token, false);
                return RedirectToAction("Index", "Home");
            }

            ModelState.AddModelError(string.Empty, response.Error);
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }

        // Controllers/AccountController.cs - Updated SignInUser method
        private async Task SignInUser(string token, bool rememberMe)
        {
            var handler = new JwtSecurityTokenHandler();

            if (!handler.CanReadToken(token))
                throw new Exception("Invalid token format");

            var jwtToken = handler.ReadJwtToken(token);

            var claims = new List<Claim>
    {
        new Claim("access_token", token)
    };

            // Add all claims from JWT (including roles)
            var roleClaims = jwtToken.Claims
                .Where(c => c.Type == "role" || c.Type == "roles")
                .Select(c => new Claim(ClaimTypes.Role, c.Value));
                 claims.AddRange(roleClaims);

            var claimsIdentity = new ClaimsIdentity(
                claims, CookieAuthenticationDefaults.AuthenticationScheme);

            var authProperties = new AuthenticationProperties
            {
                IsPersistent = rememberMe,
                ExpiresUtc = DateTimeOffset.UtcNow.AddHours(2),
                Items = { { ".Token.access_token", token } }
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);
        }
    }
}