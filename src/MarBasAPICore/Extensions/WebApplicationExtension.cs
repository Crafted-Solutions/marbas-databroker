using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;

namespace CraftedSolutions.MarBasAPICore.Extensions
{
    public static class WebApplicationExtension
    {
        public static IEnumerable<string> GetAppUrls(this WebApplication app)
        {
            var result = app.Urls;
            if (0 == result.Count)
            {
                result = app.Configuration.GetValue("Urls", string.Empty).Split(';');
            }
            return result;
        }

        public static WebApplication ConfigureHttpsRedirection(this WebApplication app)
        {
            var urls = app.GetAppUrls();
            if (urls.Any(x => x.StartsWith("https:")))
            {
                app.UseHttpsRedirection();
            }
            return app;
        }
    }
}
