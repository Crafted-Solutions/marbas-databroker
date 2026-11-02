using System.Globalization;
using CraftedSolutions.MarBasAPICore.Swagger;
using Microsoft.AspNetCore.Mvc;

namespace CraftedSolutions.MarBasAPICore.Models.Sys
{
    public sealed class LanguageQueryParametersModel
    {
        [ModelBinder(BinderType = typeof(SwaggerEnumerable<string[]>), Name = "langFilter")]
        public IEnumerable<string>? LangFilter { get; set; }

        public static IEnumerable<CultureInfo> GetCultures(IEnumerable<string> languageCodes) => languageCodes.Select(x => CultureInfo.GetCultureInfo(x));
    }
}
