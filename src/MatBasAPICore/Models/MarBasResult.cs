namespace MarBasAPICore.Models
{
    public class MarBasResult<T> : IMarBasResult<T>
    {
        public MarBasResult(bool isSuccess = true, T? payload = default)
        {
            Success = isSuccess;
            Yield = payload;
        }

        public bool Success { get; set; }

        public T? Yield { get; set; }

    }

    public static class MarbasResultFactory
    {
        public static IMarBasResult<T> Create<T>(bool isSuccess = true, T? yield = default)
        {
            return new MarBasResult<T>(isSuccess, yield);
        }
    }
}
