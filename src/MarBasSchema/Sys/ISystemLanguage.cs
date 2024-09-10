using System.Globalization;

namespace MarBasSchema.Sys
{
    public interface ISystemLanguage : ISystemLanguageRef, IUpdateable
    {
        string Label { get; set; }
        string? LabelNative { get; set; }
        CultureInfo ToCultureInfo();
    }
}
