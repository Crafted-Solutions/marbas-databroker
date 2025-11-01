using CraftedSolutions.MarBasCommon;
using CraftedSolutions.MarBasSchema.Grain;

namespace CraftedSolutions.MarBasSchema.Transport
{

    [Flags]
    public enum GrainDependencyFlags
    {
        IncludeNone = 0x0, IncludeLinks = 1 << 0, IncludeTypeDefs = 1 << 1, IncludeParent = 1 << 2, IncludeBuiltIns = 1 << 12
    }

    public static class GrainTransportableExtension
    {
        public const string FileNameFieldSeparator = ",";
        public const string GrainQualifier = "g";

        public static string MakeSerializedFileName(this Guid id, string qualifier, string fieldSeparator = FileNameFieldSeparator, string extension = ".json") => $"{qualifier}{fieldSeparator}{id:D}{extension}";

        public static string MakeSerializedFileName(this IIdentifiable grain, string fieldSeparator = FileNameFieldSeparator, string extension = ".json") => grain.Id.MakeSerializedFileName(GrainQualifier, fieldSeparator, extension);


        public static IEnumerable<IIdentifiable> GetDependencies(this IGrainTransportable grain, GrainDependencyFlags flags = GrainDependencyFlags.IncludeLinks, bool guessDetails = false)
        {
            var result = new List<IIdentifiable>();
            if (SchemaDefaults.BuiltInIds.Contains(grain.Id))
            {
                flags |= GrainDependencyFlags.IncludeBuiltIns;
            }
            if (flags.HasFlag(GrainDependencyFlags.IncludeParent) && null != grain.ParentId)
            {
                result.Add(guessDetails
                    ? new GrainPlain()
                    {
                        Id = (Guid)grain.ParentId,
                        Name = Path.GetFileName(Path.GetDirectoryName(grain.Path))!,
                        TypeDefId = SchemaDefaults.ContainerTypeDefID,
                        Path = Path.GetDirectoryName(grain.Path)!.Replace(Path.DirectorySeparatorChar, '/')
                    }
                    : (Identifiable)grain.ParentId);
            }
            if (flags.HasFlag(GrainDependencyFlags.IncludeLinks))
            {
                IIdentifiable TraitValue(ITraitTransportable trait)
                {
                    if (guessDetails)
                    {
                        var result = new GrainPlain()
                        {
                            Id = (Guid)trait.Value!,
                            TypeDefId = trait.ValueType switch
                            {
                                TraitValueType.File => SchemaDefaults.FileTypeDefID,
                                TraitValueType.Grain => SchemaDefaults.ElementTypeDefID,
                                _ => null
                            }
                        };
                        return result;
                    }
                    return (Identifiable)(Guid)trait.Value!;
                }

                var traits = Enumerable.Union(grain.Traits ?? [], grain.Localized.Values.SelectMany(x => x.Traits ?? []))
                    .Where(x => TraitValueType.Grain == x.ValueType || TraitValueType.File == x.ValueType)
                    .Select(x => TraitValue(x));
                result.AddRange(traits);

                if (grain.Tier is IGrainTierPropDef propDef && null != propDef.ValueConstraintId)
                {
                    result.Add(guessDetails ? new GrainPlain() { Id = (Guid)propDef.ValueConstraintId, TypeDefId = SchemaDefaults.ElementTypeDefID } : (Identifiable)propDef.ValueConstraintId);
                }
            }
            if (flags.HasFlag(GrainDependencyFlags.IncludeTypeDefs))
            {
                if (null == grain.TypeDefId)
                {
                    if (grain.Tier is IGrainTierTypeDef typeDef)
                    {
                        result.AddRange(typeDef.MixInIds.Select(x => (IIdentifiable)(guessDetails ? new GrainPlain() { Id = x } : (Identifiable)x)));
                    }
                }
                else
                {
                    result.Add(guessDetails ? new GrainPlain() { Id = (Guid)grain.TypeDefId } : (Identifiable)grain.TypeDefId);
                }
            }
            return flags.HasFlag(GrainDependencyFlags.IncludeBuiltIns) ? result : result.Where(x => !SchemaDefaults.BuiltInIds.Contains(x.Id));
        }
    }
}
