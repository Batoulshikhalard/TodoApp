// Services/ITodoService.cs
using Microsoft.EntityFrameworkCore;
using TodoApp.API.Data;
using TodoApp.API.Models;

namespace TodoApp.API.Services
{
    public interface ITodoService
    {
        Task<TodoItem> GetTodoByIdAsync(int id, string userId);
        Task<List<TodoItem>> GetUserTodosAsync(string userId);
        Task<TodoItem> CreateTodoAsync(TodoItem todo, string userId);
        Task<TodoItem> UpdateTodoAsync(TodoItem todo, string userId);
        Task<bool> DeleteTodoAsync(int id, string userId);
    }

    public class TodoService : ITodoService
    {
        private readonly ApplicationDbContext _context;
        private readonly IHtmlSanitizerService _sanitizer;

        public TodoService(ApplicationDbContext context, IHtmlSanitizerService sanitizer)
        {
            _context = context;
            _sanitizer = sanitizer;
        }

        public async Task<TodoItem> GetTodoByIdAsync(int id, string userId)
        {
            // Parameterized query with Entity Framework - safe from SQL injection
            return await _context.TodoItems
                .FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);
        }

        public async Task<List<TodoItem>> GetUserTodosAsync(string userId)
        {
            // Parameterized query with Entity Framework - safe from SQL injection
            return await _context.TodoItems
                .Where(t => t.UserId == userId)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();
        }

        public async Task<TodoItem> CreateTodoAsync(TodoItem todo, string userId)
        {
            // Sanitize inputs to prevent XSS
            todo.Title = _sanitizer.Sanitize(todo.Title);
            todo.Description = _sanitizer.Sanitize(todo.Description);
            todo.UserId = userId;
            todo.CreatedAt = DateTime.UtcNow;

            _context.TodoItems.Add(todo);
            await _context.SaveChangesAsync();
            return todo;
        }

        public async Task<TodoItem> UpdateTodoAsync(TodoItem todo, string userId)
        {
            var existingTodo = await GetTodoByIdAsync(todo.Id, userId);
            if (existingTodo == null)
                return null;

            // Sanitize inputs to prevent XSS
            existingTodo.Title = _sanitizer.Sanitize(todo.Title);
            existingTodo.Description = _sanitizer.Sanitize(todo.Description);
            existingTodo.IsCompleted = todo.IsCompleted;
            existingTodo.DueDate = todo.DueDate;

            await _context.SaveChangesAsync();
            return existingTodo;
        }

        public async Task<bool> DeleteTodoAsync(int id, string userId)
        {
            var todo = await GetTodoByIdAsync(id, userId);
            if (todo == null)
                return false;

            _context.TodoItems.Remove(todo);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}