namespace MarBasCommon
{
    public class NamedIdentifiable : Identifiable, INamed
    {
        protected string? _name;

        public NamedIdentifiable()
        {
        }

        public NamedIdentifiable(Guid id, string? name = null)
            : base(id)
        {
            _name = name;
        }

        public NamedIdentifiable(IIdentifiable other)
            : base(other)
        {
            if (other is INamed named)
            {
                _name = named.Name;
            }
        }

        public string Name => _name!;
    }
}
