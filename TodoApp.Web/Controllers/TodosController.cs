// Controllers/TodosController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TodoApp.Web.Models;
using TodoApp.Web.Services;

namespace TodoApp.Web.Controllers
{
    [Authorize]
    [ApiController]
    [Route("webapi/[controller]")]
    public class TodosController : ControllerBase
    {
        private readonly IApiService _apiService;

        public TodosController(IApiService apiService)
        {
            _apiService = apiService;
        }

        [HttpGet]
        public async Task<IActionResult> GetTodos()
        {
            var response = await _apiService.GetAsync<List<TodoItem>>("api/todos");

            if (!response.Success)
                return BadRequest(response.Error);

            return Ok(response.Data);
        }

        [HttpPost]
        public async Task<IActionResult> CreateTodo([FromBody] TodoItem todo)
        {
            var response = await _apiService.PostAsync<TodoItem>("api/todos", todo);

            if (!response.Success)
                return BadRequest(response.Error);

            return Ok(response.Data);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTodo(int id, [FromBody] TodoItem todo)
        {
            if (id != todo.Id)
                return BadRequest("ID mismatch");

            var response = await _apiService.PutAsync<TodoItem>($"api/todos/{id}", todo);

            if (!response.Success)
                return BadRequest(response.Error);

            return Ok(response.Data);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTodo(int id)
        {
            var response = await _apiService.DeleteAsync($"api/todos/{id}");

            if (!response.Success)
                return BadRequest(response.Error);

            return Ok();
        }
    }
}