using System.Numerics;

namespace CraftedSolutions.MarBasCommon.Reflection
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Extension methods should resemble unit literals")]
    public static class TimeSpanLiterals
    {
        public static TimeSpan h<T>(this T val) where T: INumber<T>
        {
            return val is int intVal ? TimeSpan.FromHours(intVal) : TimeSpan.FromHours((double)(object)val);
        }
        public static TimeSpan min<T>(this T val) where T: INumber<T>
        {
            return val is int intVal ? TimeSpan.FromMinutes(intVal) : TimeSpan.FromMinutes((double)(object)val);
        }
        public static TimeSpan sec<T>(this T val) where T: INumber<T>
        {
            return val is int intVal ? TimeSpan.FromSeconds(intVal) : TimeSpan.FromSeconds((double)(object)val);
        }
        public static TimeSpan ms<T>(this T val) where T : INumber<T>
        {
            return val is int intVal ? TimeSpan.FromMilliseconds(intVal) : TimeSpan.FromMilliseconds((double)(object)val);
        }
    }
}
