using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace CraftedSolutions.MarBasAPICore.Routing
{
    public class DownloadDispositionRouteConstraint : IRouteConstraint
    {
        public bool Match(HttpContext? httpContext, IRouter? route, string routeKey, RouteValueDictionary values, RouteDirection routeDirection)
        {
            var candidate = values[routeKey]?.ToString();
            return Enum.TryParse(candidate, out DownloadDisposition result);
        }
    }
}
