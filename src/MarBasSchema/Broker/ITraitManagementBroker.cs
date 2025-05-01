using System.Globalization;
using CraftedSolutions.MarBasCommon;
using CraftedSolutions.MarBasSchema.Grain;

namespace CraftedSolutions.MarBasSchema.Broker
{
    public interface ITraitManagementBroker
    {
        GrainTraitsMap GetGrainTraits(IIdentifiable grain, CultureInfo? culture = null);
        ITraitBase? GetTrait(Guid id);
        int DeleteTraits(IEnumerable<IIdentifiable> ids);
        int StoreTraits(IEnumerable<ITraitBase> traits);
        ITraitBase? CreateTrait(ITraitRef traitRef, object? value = null, int ord = 0);
        IEnumerable<ITraitBase> GetTraitValues(ITraitRef traitRef);
        int ReplaceTraitValues<T>(ITraitRef traitRef, IEnumerable<T> values);
        int ResetTraitValues(ITraitRef traitRef);
        int ReindexTraits(IIdentifiable grain, IIdentifiable? propDef = null, CultureInfo? culture = null, int revision = -1, bool trimOverflow = false);
        IEnumerable<IGrainLocalized> LookupGrainsByTrait(ITraitRef traitRef, object? value = null, IEnumerable<IListSortOption<GrainSortField>>? sortOptions = null);
    }
}
