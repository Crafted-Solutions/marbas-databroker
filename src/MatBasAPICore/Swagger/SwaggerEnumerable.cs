using System.Collections;
using System.Text.Json;
using MarBasCommon.Reflection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Options;

namespace MarBasAPICore.Swagger
{
    public sealed class SwaggerEnumerable<T> : IModelBinder where T : IEnumerable
    {
        private readonly IOptions<JsonOptions> _options;

        public SwaggerEnumerable(IOptions<JsonOptions> options)
        {
            _options = options;
        }

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
                result += type.IsPrimitive || typeof(Decimal).IsAssignableFrom(type) || v.StartsWith('{') || v.StartsWith('[')
                    ? $"{v}" : $"\"{v}\"";
                return result;
            });
            var model = JsonSerializer.Deserialize<T>($"[{json}]", _options.Value.JsonSerializerOptions);
            bindingContext.Result = ModelBindingResult.Success(model);
            return Task.CompletedTask;
        }

    }
}
