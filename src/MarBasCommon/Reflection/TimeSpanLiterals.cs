namespace CraftedSolutions.MarBasCommon.Reflection
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Extension methods should resemble unit literals")]
    public static class TimeSpanLiterals
    {
        public static TimeSpan h(this double val)
        {
            return TimeSpan.FromHours(val);
        }
        public static TimeSpan h(this int val)
        {
            return TimeSpan.FromHours(val);
        }
        public static TimeSpan min(this double val)
        {
            return TimeSpan.FromMinutes(val);
        }
        public static TimeSpan min(this int val)
        {
            return TimeSpan.FromMinutes(val);
        }
        public static TimeSpan sec(this double val)
        {
            return TimeSpan.FromSeconds(val);
        }
        public static TimeSpan sec(this int val)
        {
            return TimeSpan.FromSeconds(val);
        }
        public static TimeSpan ms(this double val)
        {
            return TimeSpan.FromMilliseconds(val);
        }
        public static TimeSpan ms(this int val)
        {
            return TimeSpan.FromMilliseconds(val);
        }
    }
}
