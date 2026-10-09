using CraftedSolutions.MarBasCommon;
using CraftedSolutions.MarBasSchema.Grain;
using System.Globalization;

namespace CraftedSolutions.MarBasSchema.Tests.Grain.Traits
{
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    internal class TraitMock : ITrait
    {
        public int Ord { get; set; }

        public bool IsNull => null == Value;

        public object? Value { get; set; }

        public Guid Id { get; }

        public TraitValueType ValueType { get; set; }

        public IIdentifiable PropDef { get; set; } = new Identifiable();

        public Guid PropDefId => PropDef.Id;

        public int Revision { get; set; }
        public IIdentifiable Grain { get; set; } = new Identifiable();

        public Guid GrainId => Grain.Id;

        public CultureInfo? CultureInfo { get; set; }

        public string? Culture => CultureInfo?.Name;
    }
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    internal class TraitBaseMock(IIdentifiable grain, IIdentifiable propDef, CultureInfo? culture = null)
        : TraitBase(grain, propDef, culture)
    {
        public object? OwnValue { get; set; }
        public override object? Value => OwnValue;
    }
}
