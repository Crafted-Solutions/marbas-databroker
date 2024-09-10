namespace MarBasSchema.Grain
{
    public interface ITraitValue<T>: ITrait
    {
        new T? Value { get; set; }
    }
}
