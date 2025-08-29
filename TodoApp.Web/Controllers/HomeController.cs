// Controllers/HomeController.cs
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TodoApp.Web.Models;
using TodoApp.Web.Services;

namespace TodoApp.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly IApiService _apiService;

        public HomeController(IApiService apiService)
        {
            _apiService = apiService;
        }

        public IActionResult Index()
        {
            return View();
        }

        [Authorize]
        public async Task<IActionResult> Dashboard()
        {
            var isAuthenticated = await _apiService.IsAuthenticatedAsync();
            if (!isAuthenticated)
            {
                return RedirectToAction("Login", "Account");
            }

            return View();
        }

        [Authorize]
        public async Task<IActionResult> Todos()
        {
            var response = await _apiService.GetAsync<List<TodoItem>>("api/todos");

            if (!response.Success)
            {
                // Handle error
                return View(new List<TodoItem>());
            }

            return View(response.Data);
        }
    }
}