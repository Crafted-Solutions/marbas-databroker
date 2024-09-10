namespace MarBasSchema.Broker
{
    public interface IListSortOption<TFieldEnum>
        where TFieldEnum : struct, Enum
    {
        TFieldEnum Field { get; set; }
        ListSortOrder Order { get; set; }
    }
}
