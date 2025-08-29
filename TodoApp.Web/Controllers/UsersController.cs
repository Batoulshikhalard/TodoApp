// Controllers/UsersController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TodoApp.Web.Models;
using TodoApp.Web.Services;

namespace TodoApp.Web.Controllers;

[Authorize]
public class UsersController : Controller
{
    private readonly IApiService _apiService;
    private readonly ILogger<UsersController> _logger;

    public UsersController(IApiService apiService, ILogger<UsersController> logger)
    {
        _apiService = apiService;
        _logger = logger;
    }

    // GET: Users
    public async Task<IActionResult> Index()
    {
        try
        {
            var response = await _apiService.GetAsync<List<UserViewModel>>("api/users");

            if (!response.Success)
            {
                TempData["ErrorMessage"] = response.Error;
                return View(new List<UserViewModel>());
            }

            return View(response.Data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading users");
            TempData["ErrorMessage"] = "Error loading users";
            return View(new List<UserViewModel>());
        }
    }

    // GET: Users/Details/5
    public async Task<IActionResult> Details(string id)
    {
        try
        {
            var response = await _apiService.GetAsync<UserViewModel>($"api/users/{id}");

            if (!response.Success)
            {
                TempData["ErrorMessage"] = response.Error;
                return RedirectToAction(nameof(Index));
            }

            return View(response.Data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading user details");
            TempData["ErrorMessage"] = "Error loading user details";
            return RedirectToAction(nameof(Index));
        }
    }

    // GET: Users/Edit/5
    public async Task<IActionResult> Edit(string id)
    {
        try
        {
            // Get user
            var userResponse = await _apiService.GetAsync<UpdateUserModel>($"api/users/{id}");
            if (!userResponse.Success)
            {
                TempData["ErrorMessage"] = userResponse.Error;
                return RedirectToAction(nameof(Index));
            }

            // Get available roles
            var rolesResponse = await _apiService.GetAsync<List<string>>("api/users/roles");
            if (!rolesResponse.Success)
            {
                TempData["ErrorMessage"] = rolesResponse.Error;
                return RedirectToAction(nameof(Index));
            }

            ViewBag.AvailableRoles = rolesResponse.Data;
            return View(userResponse.Data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading user for edit");
            TempData["ErrorMessage"] = "Error loading user for edit";
            return RedirectToAction(nameof(Index));
        }
    }

    // POST: Users/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(string id, UpdateUserModel model)
    {
        try
        {
            if (id != model.Id)
            {
                TempData["ErrorMessage"] = "ID mismatch";
                return RedirectToAction(nameof(Index));
            }

            if (!ModelState.IsValid)
            {
                // Get available roles for the view
                var rolesResponse = await _apiService.GetAsync<List<string>>("api/users/roles");
                if (rolesResponse.Success)
                {
                    ViewBag.AvailableRoles = rolesResponse.Data;
                }
                return View(model);
            }

            var response = await _apiService.PutAsync<object>($"api/users/{id}", model);

            if (!response.Success)
            {
                TempData["ErrorMessage"] = response.Error;

                // Get available roles for the view
                var rolesResponse = await _apiService.GetAsync<List<string>>("api/users/roles");
                if (rolesResponse.Success)
                {
                    ViewBag.AvailableRoles = rolesResponse.Data;
                }
                return View(model);
            }

            TempData["SuccessMessage"] = "User updated successfully";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user");
            TempData["ErrorMessage"] = "Error updating user";
            return RedirectToAction(nameof(Index));
        }
    }

    // GET: Users/ChangePassword/5
    public async Task<IActionResult> ChangePassword(string id)
    {
        try
        {
            var response = await _apiService.GetAsync<UserViewModel>($"api/users/{id}");

            if (!response.Success)
            {
                TempData["ErrorMessage"] = response.Error;
                return RedirectToAction(nameof(Index));
            }

            var model = new ChangePasswordModel
            {
                UserId = response.Data.Id
            };

            return View(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading user for password change");
            TempData["ErrorMessage"] = "Error loading user for password change";
            return RedirectToAction(nameof(Index));
        }
    }

    // POST: Users/ChangePassword/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangePassword(string id, ChangePasswordModel model)
    {
        try
        {
            if (id != model.UserId)
            {
                TempData["ErrorMessage"] = "ID mismatch";
                return RedirectToAction(nameof(Index));
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var response = await _apiService.PostAsync<object>($"api/users/{id}/changepassword", model);

            if (!response.Success)
            {
                TempData["ErrorMessage"] = response.Error;
                return View(model);
            }

            TempData["SuccessMessage"] = "Password changed successfully";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error changing password");
            TempData["ErrorMessage"] = "Error changing password";
            return RedirectToAction(nameof(Index));
        }
    }

    // POST: Users/Delete/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(string id)
    {
        try
        {
            var response = await _apiService.DeleteAsync($"api/users/{id}");

            if (!response.Success)
            {
                TempData["ErrorMessage"] = response.Error;
            }
            else
            {
                TempData["SuccessMessage"] = "User deleted successfully";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting user");
            TempData["ErrorMessage"] = "Error deleting user";
        }

        return RedirectToAction(nameof(Index));
    }

    // POST: Users/ToggleStatus/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleStatus(string id)
    {
        try
        {
            // Get current user data
            var userResponse = await _apiService.GetAsync<UserViewModel>($"api/users/{id}");
            if (!userResponse.Success)
            {
                TempData["ErrorMessage"] = userResponse.Error;
                return RedirectToAction(nameof(Index));
            }

            // Toggle status
            var updateModel = new UpdateUserModel
            {
                Id = userResponse.Data.Id,
                Email = userResponse.Data.Email,
                FirstName = userResponse.Data.FirstName,
                LastName = userResponse.Data.LastName,
                IsActive = !userResponse.Data.IsActive,
                Roles = userResponse.Data.Roles
            };

            var updateResponse = await _apiService.PutAsync<object>($"api/users/{id}", updateModel);

            if (!updateResponse.Success)
            {
                TempData["ErrorMessage"] = updateResponse.Error;
            }
            else
            {
                TempData["SuccessMessage"] = $"User {(updateModel.IsActive ? "activated" : "deactivated")} successfully";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error toggling user status");
            TempData["ErrorMessage"] = "Error toggling user status";
        }

        return RedirectToAction(nameof(Index));
    }
}