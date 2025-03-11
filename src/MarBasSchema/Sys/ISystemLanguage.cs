using System.Globalization;
using CraftedSolutions.MarBasSchema;

namespace CraftedSolutions.MarBasSchema.Sys
{
    public interface ISystemLanguage : ISystemLanguageRef, IUpdateable
    {
        string Label { get; set; }
        string? LabelNative { get; set; }
        CultureInfo ToCultureInfo();
    }
}
