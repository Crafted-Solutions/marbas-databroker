using MarBasCommon;

namespace MarBasSchema
{
    public class SimpleTypeConstraint : ITypeConstraint
    {
        protected IIdentifiable? _typeDef;

        public SimpleTypeConstraint()
        {
        }

        public SimpleTypeConstraint(Guid typeDefId, string? typeName = null)
        {
            _typeDef = new NamedIdentifiable(typeDefId, typeName);
        }

        public SimpleTypeConstraint(ITypeConstraint other)
        {
            _typeDef = other.TypeDef;
        }

        public Guid? TypeDefId
        {
            get => null == _typeDef?.Id || Guid.Empty.Equals(_typeDef?.Id) ? null : _typeDef?.Id;
            set => _typeDef = (Identifiable?)value;
        }

        public IIdentifiable? TypeDef { get => _typeDef; set => _typeDef = value; }

        public string? TypeName => _typeDef is INamed named ? named.Name : null;

        public static SimpleTypeConstraint CreateFrom<T>(T typeDef) where T : IIdentifiable, INamed
        {
            var result = new SimpleTypeConstraint
            {
                _typeDef = typeDef
            };
            return result;
        }
    }
}
