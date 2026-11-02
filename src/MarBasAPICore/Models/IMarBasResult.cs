namespace CraftedSolutions.MarBasAPICore.Models
{
    public interface IMarBasResult<T>
    {
        bool Success { get; }
        T? Yield { get; }
    }
}
