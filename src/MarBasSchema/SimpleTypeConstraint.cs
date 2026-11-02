using CraftedSolutions.MarBasCommon;

namespace CraftedSolutions.MarBasSchema
{
    public class SimpleTypeConstraint : ITypeConstraint, IEquatable<ITypeConstraint>
    {
        protected IIdentifiable? _typeDef;

        public SimpleTypeConstraint()
        {
        }

        public SimpleTypeConstraint(IIdentifiable? typeDef)
        {
            _typeDef = typeDef;
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
            get => null == _typeDef?.Id || Guid.Empty.Equals(_typeDef.Id) ? null : _typeDef.Id;
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

        public override bool Equals(object? obj)
        {
            return Equals(obj as ITypeConstraint);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(TypeDefId, TypeName);
        }

        public bool Equals(ITypeConstraint? other)
        {
            if (other is null)
            {
                return false;
            }
            if (ReferenceEquals(this, other))
            {
                return true;
            }
            return TypeDefId == other.TypeDefId && TypeName == other.TypeName;
        }

        public static bool operator ==(SimpleTypeConstraint? lhs, SimpleTypeConstraint? rhs)
        {
            return lhs == (ITypeConstraint?) rhs;
        }

        public static bool operator !=(SimpleTypeConstraint? lhs, SimpleTypeConstraint? rhs)
        {
            return lhs != (ITypeConstraint?) rhs;
        }

        public static bool operator ==(SimpleTypeConstraint? lhs, ITypeConstraint? rhs)
        {
            if (ReferenceEquals(lhs, rhs))
            {
                return true;
            }
            if (lhs is null || rhs is null)
            {
                return false;
            }
            return lhs.TypeDefId == rhs.TypeDefId && lhs.TypeName == rhs.TypeName;
        }

        public static bool operator !=(SimpleTypeConstraint? lhs, ITypeConstraint? rhs)
        {
            return !(lhs == rhs);
        }
    }
}
