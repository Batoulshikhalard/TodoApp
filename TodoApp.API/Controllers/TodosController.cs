// Controllers/TodosController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TodoApp.API.Data;
using TodoApp.API.Dtos;
using TodoApp.API.Models;

namespace TodoApp.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class TodosController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public TodosController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> GetTodos()
        {
            var userId = _userManager.GetUserId(User);
            var todos = await _context.TodoItems
                .Where(t => t.UserId == userId)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();

            return Ok(todos);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetTodo(int id)
        {
            var userId = _userManager.GetUserId(User);
            var todo = await _context.TodoItems
                .FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);

            if (todo == null)
                return NotFound();

            return Ok(todo);
        }

        [HttpPost]
        public async Task<IActionResult> CreateTodo([FromBody] TodoDto todo)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            //var userId = _userManager.GetUserId(User);

            // Get user ID from JWT claims
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("User ID not found in token");
            }

            todo.UserId = userId;
            todo.CreatedAt = DateTime.UtcNow;

            var todoItem = new TodoItem
            {
                Title = todo.Title,
                DueDate = DateTime.UtcNow,
                Description = todo.Description,
                CreatedAt = DateTime.UtcNow,
                IsCompleted = todo.IsCompleted,
                UserId = userId,
            };

            _context.TodoItems.Add(todoItem);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetTodo), new { id = todoItem.Id }, todoItem);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTodo(int id, [FromBody] TodoItem updatedTodo)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (id != updatedTodo.Id)
                return BadRequest("ID mismatch");

            var userId = _userManager.GetUserId(User);
            var todo = await _context.TodoItems
                .FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);

            if (todo == null)
                return NotFound();

            todo.Title = updatedTodo.Title;
            todo.Description = updatedTodo.Description;
            todo.IsCompleted = updatedTodo.IsCompleted;
            todo.DueDate = updatedTodo.DueDate;

            await _context.SaveChangesAsync();

            return Ok(todo);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTodo(int id)
        {
            var userId = _userManager.GetUserId(User);
            var todo = await _context.TodoItems
                .FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);

            if (todo == null)
                return NotFound();

            _context.TodoItems.Remove(todo);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}