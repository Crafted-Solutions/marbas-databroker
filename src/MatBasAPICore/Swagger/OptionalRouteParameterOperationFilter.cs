using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Text.RegularExpressions;

namespace MarBasAPICore.Swagger
{
    public class OptionalRouteParameterOperationFilter : IOperationFilter
    {
        private const string CaptureName = "routeParameter";

        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            var httpMethodAttributes = context.MethodInfo
                    .GetCustomAttributes(true)
                    .OfType<Microsoft.AspNetCore.Mvc.Routing.HttpMethodAttribute>();

            var httpMethodWithOptional = httpMethodAttributes?.FirstOrDefault(m => m.Template?.Contains('?') ?? false);
            if (httpMethodWithOptional == null)
            {
                return;
            }

            string regex = $"{{(?<{CaptureName}>\\w+)\\?}}";

            var matches = Regex.Matches(httpMethodWithOptional.Template!, regex);

            foreach (var match in (IList<Match>)matches)
            {
                var name = match.Groups[CaptureName].Value;

                var parameter = operation.Parameters.FirstOrDefault(p => p.In == ParameterLocation.Path && p.Name == name);
                if (parameter != null)
                {
                    parameter.AllowEmptyValue = true;
                    parameter.Description = "Check \"Send empty value\" if not using parameter";
                    parameter.Required = false;
                    //parameter.Schema.Default = new OpenApiString(string.Empty);
                    parameter.Schema.Nullable = true;
                }
            }
        }
    }
}
