// Controllers/UsersController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TodoApp.API.Dtos;
using TodoApp.API.Models;

namespace TodoApp.API.Controllers
{
    [Authorize(AuthenticationSchemes = "Bearer")]
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ILogger<UsersController> _logger;

        public UsersController(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            ILogger<UsersController> logger)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _logger = logger;
        }

        // GET: api/users (Admin only)
        //[Authorize(Roles = "Admin")]
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetUsers()
        {
            try
            {
                var users = await _userManager.Users
                    .OrderBy(u => u.CreatedAt)
                    .ToListAsync();

                var userViewModels = new List<UserViewModel>();

                foreach (var user in users)
                {
                    var roles = await _userManager.GetRolesAsync(user);
                    userViewModels.Add(new UserViewModel
                    {
                        Id = user.Id,
                        UserName = user.UserName,
                        Email = user.Email,
                        FirstName = user.FirstName,
                        LastName = user.LastName,
                        CreatedAt = user.CreatedAt,
                        IsActive = user.IsActive,
                        Roles = roles.ToList()
                    });
                }

                return Ok(userViewModels);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting users");
                return StatusCode(500, "Internal server error");
            }
        }

        // GET: api/users/{id}
        [Authorize]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetUser(string id)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(id);
                if (user == null)
                {
                    return NotFound("User not found");
                }

                var roles = await _userManager.GetRolesAsync(user);
                var userViewModel = new UserViewModel
                {
                    Id = user.Id,
                    UserName = user.UserName,
                    Email = user.Email,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    CreatedAt = user.CreatedAt,
                    IsActive = user.IsActive,
                    Roles = roles.ToList()
                };

                return Ok(userViewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user");
                return StatusCode(500, "Internal server error");
            }
        }

        // PUT: api/users/{id}
        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(string id, [FromBody] UpdateUserModel model)
        {
            try
            {
                if (id != model.Id)
                {
                    return BadRequest("ID mismatch");
                }

                var user = await _userManager.FindByIdAsync(id);
                if (user == null)
                {
                    return NotFound("User not found");
                }

                // Update user properties
                user.Email = model.Email;
                user.UserName = model.Email; // Keep username in sync with email
                user.FirstName = model.FirstName;
                user.LastName = model.LastName;
                user.IsActive = model.IsActive;

                var updateResult = await _userManager.UpdateAsync(user);
                if (!updateResult.Succeeded)
                {
                    return BadRequest(updateResult.Errors);
                }

                // Update roles
                var currentRoles = await _userManager.GetRolesAsync(user);
                var removeResult = await _userManager.RemoveFromRolesAsync(user, currentRoles);
                if (!removeResult.Succeeded)
                {
                    return BadRequest(removeResult.Errors);
                }

                if (model.Roles.Any())
                {
                    var addResult = await _userManager.AddToRolesAsync(user, model.Roles);
                    if (!addResult.Succeeded)
                    {
                        return BadRequest(addResult.Errors);
                    }
                }

                return Ok(new { message = "User updated successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user");
                return StatusCode(500, "Internal server error");
            }
        }

        // DELETE: api/users/{id}
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(string id)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(id);
                if (user == null)
                {
                    return NotFound("User not found");
                }

                // Prevent admin from deleting themselves
                var currentUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (user.Id == currentUserId)
                {
                    return BadRequest("Cannot delete your own account");
                }

                var result = await _userManager.DeleteAsync(user);
                if (!result.Succeeded)
                {
                    return BadRequest(result.Errors);
                }

                return Ok(new { message = "User deleted successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting user");
                return StatusCode(500, "Internal server error");
            }
        }

        // POST: api/users/{id}/changepassword
        [Authorize(Roles = "Admin")]
        [HttpPost("{id}/changepassword")]
        public async Task<IActionResult> ChangePassword(string id, [FromBody] ChangePasswordModel model)
        {
            try
            {
                if (id != model.UserId)
                {
                    return BadRequest("ID mismatch");
                }

                var user = await _userManager.FindByIdAsync(id);
                if (user == null)
                {
                    return NotFound("User not found");
                }

                // Generate a password reset token and set new password
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                var result = await _userManager.ResetPasswordAsync(user, token, model.NewPassword);

                if (!result.Succeeded)
                {
                    return BadRequest(result.Errors);
                }

                return Ok(new { message = "Password changed successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error changing password");
                return StatusCode(500, "Internal server error");
            }
        }

        // GET: api/users/roles
        [Authorize(Roles = "Admin")]
        [HttpGet("roles")]
        public IActionResult GetRoles()
        {
            try
            {
                var roles = _roleManager.Roles.Select(r => r.Name).ToList();
                return Ok(roles);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting roles");
                return StatusCode(500, "Internal server error");
            }
        }
    }
}