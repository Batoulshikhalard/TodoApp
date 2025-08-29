// Services/HtmlSanitizerService.cs
using Ganss.Xss;

namespace TodoApp.Web.Services
{
    public interface IHtmlSanitizerService
    {
        string Sanitize(string input);
    }

    public class HtmlSanitizerService : IHtmlSanitizerService
    {
        private readonly HtmlSanitizer _sanitizer;

        public HtmlSanitizerService()
        {
            _sanitizer = new HtmlSanitizer();
            _sanitizer.AllowedTags.Add("b");
            _sanitizer.AllowedTags.Add("i");
            _sanitizer.AllowedTags.Add("u");
            _sanitizer.AllowedTags.Add("br");
            _sanitizer.AllowedTags.Add("p");
            _sanitizer.AllowedAttributes.Add("class");
        }

        public string Sanitize(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            return _sanitizer.Sanitize(input);
        }
    }
}