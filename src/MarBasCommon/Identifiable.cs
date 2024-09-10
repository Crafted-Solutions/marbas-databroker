namespace MarBasCommon
{
    public class Identifiable : IIdentifiable
    {
        protected Guid _id;

        public Identifiable()
        {
            _id = Guid.NewGuid();
        }

        public Identifiable(Guid id)
        {
            _id = id;
        }

        public Identifiable(IIdentifiable other)
        {
            _id = other.Id;
        }

        public Guid Id => _id;

        public static implicit operator Identifiable(Guid guid) => new(guid);
        public static implicit operator Guid(Identifiable identifiable) => identifiable.Id;
    }
}
