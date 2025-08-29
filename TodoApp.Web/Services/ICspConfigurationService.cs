// Services/CspConfigurationService.cs
namespace TodoApp.Web.Services
{
    public interface ICspConfigurationService
    {
        string GetCspHeader();
    }

    public class CspConfigurationService : ICspConfigurationService
    {
        private readonly IWebHostEnvironment _environment;

        public CspConfigurationService(IWebHostEnvironment environment)
        {
            _environment = environment;
        }

        public string GetCspHeader()
        {
            if (_environment.IsDevelopment())
            {
                return "default-src 'self' ws: wss: http://localhost:*; " +
                       "connect-src 'self' ws: wss: http://localhost:*; " +
                       "script-src 'self' 'unsafe-inline' 'unsafe-eval'; " +
                       "style-src 'self' 'unsafe-inline'; " +
                       "img-src 'self' data: blob:; " +
                       "font-src 'self' data:; " +
                       "frame-ancestors 'none';";
            }
            else
            {
                return "default-src 'self'; " +
                       "script-src 'self'; " +
                       "style-src 'self'; " +
                       "img-src 'self' data:; " +
                       "font-src 'self'; " +
                       "frame-ancestors 'none';";
            }
        }
    }
}