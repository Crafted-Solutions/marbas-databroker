namespace MarBasSchema.Broker
{
    public class ListSortOption<TFieldEnum> : IListSortOption<TFieldEnum>
        where TFieldEnum : struct, Enum
    {
        public ListSortOption()
        {
        }

        public ListSortOption(TFieldEnum field, ListSortOrder order)
        {
            Field = field;
            Order = order;
        }

        public TFieldEnum Field { get; set; }
        public ListSortOrder Order { get; set; }
    }
}
