// Services/ApiService.cs
using Microsoft.AspNetCore.Authentication;
using TodoApp.Web.Models;

namespace TodoApp.Web.Services;

public class ApiService : IApiService
{
    private readonly HttpClient _httpClient;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IConfiguration _configuration;

    public ApiService(
        HttpClient httpClient,
        IHttpContextAccessor httpContextAccessor,
        IConfiguration configuration)
    {
        _httpClient = httpClient;
        _httpContextAccessor = httpContextAccessor;
        _configuration = configuration;

        _httpClient.BaseAddress = new Uri(_configuration["ApiSettings:BaseUrl"]);
    }

    //public async Task<string> GetTokenAsync()
    //{
    //    return await _httpContextAccessor.HttpContext.GetTokenAsync("access_token");
    //}

    public Task<string> GetTokenAsync()
    {
        var token = _httpContextAccessor.HttpContext?.User?.FindFirst("access_token")?.Value;
        return Task.FromResult(token);
    }


    public async Task<string> GetXsrfTokenAsync()
    {
        // Get XSRF token from cookie
        return _httpContextAccessor.HttpContext.Request.Cookies["XSRF-TOKEN"];
    }

    public Task<bool> IsAuthenticatedAsync()
    {
        var user = _httpContextAccessor.HttpContext?.User;
        return Task.FromResult(user?.Identity?.IsAuthenticated ?? false);
    }

    private async Task<HttpRequestMessage> CreateRequestAsync(HttpMethod method, string endpoint, object data = null)
    {
        var request = new HttpRequestMessage(method, endpoint);

        // Add JWT token if available
        var token = await GetTokenAsync();
        if (!string.IsNullOrEmpty(token))
        {
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        }

        // Add XSRF token for state-changing requests
        if (method == HttpMethod.Post || method == HttpMethod.Put || method == HttpMethod.Delete)
        {
            var xsrfToken = await GetXsrfTokenAsync();
            if (!string.IsNullOrEmpty(xsrfToken))
            {
                request.Headers.Add("X-XSRF-TOKEN", xsrfToken);
            }
        }

        // Add JSON content for POST/PUT requests
        if (data != null && (method == HttpMethod.Post || method == HttpMethod.Put))
        {
            var json = System.Text.Json.JsonSerializer.Serialize(data);
            request.Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
        }

        return request;
    }

    public async Task<ApiResponse<T>> GetAsync<T>(string endpoint)
    {
        try
        {
            var request = await CreateRequestAsync(HttpMethod.Get, endpoint);
            var response = await _httpClient.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var data = System.Text.Json.JsonSerializer.Deserialize<T>(content, new System.Text.Json.JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                return new ApiResponse<T> { Success = true, Data = data };
            }

            return new ApiResponse<T> { Success = false, Error = $"API error: {response.StatusCode}" };
        }
        catch (Exception ex)
        {
            return new ApiResponse<T> { Success = false, Error = ex.Message };
        }
    }

    public async Task<ApiResponse<T>> PostAsync<T>(string endpoint, object data)
    {
        try
        {
            var request = await CreateRequestAsync(HttpMethod.Post, endpoint, data);
            var response = await _httpClient.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var result = System.Text.Json.JsonSerializer.Deserialize<T>(content, new System.Text.Json.JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                return new ApiResponse<T> { Success = true, Data = result };
            }

            return new ApiResponse<T> { Success = false, Error = $"API error: {response.StatusCode}" };
        }
        catch (Exception ex)
        {
            return new ApiResponse<T> { Success = false, Error = ex.Message };
        }
    }

    public async Task<ApiResponse<T>> PutAsync<T>(string endpoint, object data)
    {
        try
        {
            var request = await CreateRequestAsync(HttpMethod.Put, endpoint, data);
            var response = await _httpClient.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var result = System.Text.Json.JsonSerializer.Deserialize<T>(content, new System.Text.Json.JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                return new ApiResponse<T> { Success = true, Data = result };
            }

            return new ApiResponse<T> { Success = false, Error = $"API error: {response.StatusCode}" };
        }
        catch (Exception ex)
        {
            return new ApiResponse<T> { Success = false, Error = ex.Message };
        }
    }

    public async Task<ApiResponse<bool>> DeleteAsync(string endpoint)
    {
        try
        {
            var request = await CreateRequestAsync(HttpMethod.Delete, endpoint);
            var response = await _httpClient.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                return new ApiResponse<bool> { Success = true, Data = true };
            }

            return new ApiResponse<bool> { Success = false, Error = $"API error: {response.StatusCode}" };
        }
        catch (Exception ex)
        {
            return new ApiResponse<bool> { Success = false, Error = ex.Message };
        }
    }
}