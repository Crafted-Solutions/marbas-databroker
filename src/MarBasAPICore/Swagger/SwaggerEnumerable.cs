using System.Collections;
using System.Text.Json;
using CraftedSolutions.MarBasCommon.Reflection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Options;

namespace CraftedSolutions.MarBasAPICore.Swagger
{
    public sealed class SwaggerEnumerable<T>(IOptions<JsonOptions> options) : IModelBinder where T : IEnumerable
    {
        private readonly JsonSerializerOptions _serializerOptions = options.Value.JsonSerializerOptions;

        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            var val = bindingContext.ValueProvider.GetValue(bindingContext.FieldName);
            var type = bindingContext.ModelType.GetEnumerableType();
            var json = val.Aggregate(string.Empty, (aggr, element) =>
            {
                var result = aggr;
                if (0 < aggr.Length)
                {
                    result += ",";
                }
                var v = element.Trim();
                result += type.IsPrimitive || typeof(decimal).IsAssignableFrom(type) || v.StartsWith('{') || v.StartsWith('[')
                    ? $"{v}" : $"\"{v}\"";
                return result;
            });
            var model = JsonSerializer.Deserialize<T>($"[{json}]", _serializerOptions);
            bindingContext.Result = ModelBindingResult.Success(model);
            return Task.CompletedTask;
        }

    }
}
