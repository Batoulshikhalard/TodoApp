using TodoApp.Web.Models;

namespace TodoApp.Web.Services;

public interface IApiService
{
    Task<string> GetTokenAsync();
    Task<string> GetXsrfTokenAsync();
    Task<bool> IsAuthenticatedAsync();
    Task<ApiResponse<T>> GetAsync<T>(string endpoint);
    Task<ApiResponse<T>> PostAsync<T>(string endpoint, object data);
    Task<ApiResponse<T>> PutAsync<T>(string endpoint, object data);
    Task<ApiResponse<bool>> DeleteAsync(string endpoint);
}