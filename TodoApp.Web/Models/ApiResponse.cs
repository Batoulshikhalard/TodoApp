namespace TodoApp.Web.Models;

public class ApiResponse<T>
{
    public bool Success { get; set; }
    public T Data { get; set; }
    public string Error { get; set; }
}

public class AuthResponse
{
    public string Token { get; set; }
}
