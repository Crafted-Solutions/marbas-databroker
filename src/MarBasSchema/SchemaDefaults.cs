using System.Globalization;
using System.Security.Principal;
using MarBasSchema.Access;
using MarBasSchema.GrainDef;
using MarBasSchema.GrainTier;

namespace MarBasSchema
{
    public static class SchemaDefaults
    {
        public static readonly CultureInfo Culture = CultureInfo.GetCultureInfo("en");

        public static readonly Guid AnyGrainID = Guid.Parse("00000000-0000-1000-a000-000000000000");

        public static readonly Guid RootID = Guid.Parse("00000000-0000-1000-a000-000000000001");
        public const string RootName = "marbas";

        public static readonly Guid TypeDefTypeDefID;
        public const string TypeDefTypeName = "Type";

        public static readonly Guid PropDefTypeDefID = Guid.Parse("00000000-0000-1000-a000-000000000009");
        public const string PropDefTypeName = "Property";

        public static readonly Guid FileTypeDefID = Guid.Parse("00000000-0000-1000-a000-00000000000a");
        public const string FileTypeName = "File";

        public static readonly Guid TrashbinTypeDefID = Guid.Parse("00000000-0000-1000-a000-00000000000e");

        public static readonly Guid SchemaContainerID = Guid.Parse("00000000-0000-1000-a000-000000000002");
        public static readonly Guid ContentContainerID = Guid.Parse("00000000-0000-1000-a000-000000000006");
        public static readonly Guid FilesContainerID = Guid.Parse("00000000-0000-1000-a000-000000000008");
        public static readonly Guid SystemSchemaContainerID = Guid.Parse("00000000-0000-1000-a000-000000000003");
        public static readonly Guid UserSchemaContainerID = Guid.Parse("00000000-0000-1000-a000-000000000007");
        public static readonly Guid ContentTrashID = Guid.Parse("00000000-0000-1000-a000-000000000010");
        public static readonly Guid SchemaTrashID = Guid.Parse("00000000-0000-1000-a000-000000000011");
        public static readonly Guid PropDefCommentID = Guid.Parse("00000000-0000-1000-a000-00000000000d");

        public static readonly Guid ElementTypeDefID = Guid.Parse("00000000-0000-1000-a000-000000000004");
        public static readonly Guid ContainerTypeDefID = Guid.Parse("00000000-0000-1000-a000-000000000005");


        public static readonly Guid SuperuserRoleID = Guid.Parse("00000000-0000-1000-b000-000000000000");
        public static readonly Guid DeveloperRoleID = Guid.Parse("00000000-0000-1000-b000-000000000001");
        public static readonly Guid SchemaManagerRoleID = Guid.Parse("00000000-0000-1000-b000-000000000002");
        public static readonly Guid ContentContributorRoleID = Guid.Parse("00000000-0000-1000-b000-000000000003");
        public static readonly Guid ContentConsumerRoleID = Guid.Parse("00000000-0000-1000-b000-000000000004");
        public static readonly Guid EveryoneRoleID = Guid.Parse("00000000-0000-1000-b000-000000000005");

        public const string SystemPrincipalSuffix = "@marbas";

        public const string AnonymousUserName = "anonymous";
        public const string SystemUserName = $"system{SystemPrincipalSuffix}";
        public static readonly IPrincipal AnonymousUser = new GenericPrincipal(new GenericIdentity(AnonymousUserName), new string[] { SchemaRole.Everyone.Name });
        public static readonly IPrincipal SystemUser = new GenericPrincipal(new GenericIdentity(SystemUserName), new string[] { SchemaRole.Superuser.Name });


        public static readonly ISet<Guid> BuiltInIds = new HashSet<Guid>()
        {
            AnyGrainID,
            RootID,
            SchemaContainerID,
            SystemSchemaContainerID,
            UserSchemaContainerID,
            ContentContainerID,
            FilesContainerID,
            ContentTrashID,
            SchemaTrashID,
            ElementTypeDefID,
            ContainerTypeDefID,
            PropDefTypeDefID,
            PropDefCommentID,
            FileTypeDefID,
            TrashbinTypeDefID,
            SuperuserRoleID,
            DeveloperRoleID,
            SchemaManagerRoleID,
            ContentContributorRoleID,
            ContentConsumerRoleID,
            EveryoneRoleID
        };

        public static readonly ISet<(Guid, Guid)> BuiltInAcl = new HashSet<(Guid, Guid)>()
        {
            (AnyGrainID, SuperuserRoleID),
            (AnyGrainID, DeveloperRoleID),
            (AnyGrainID, SchemaManagerRoleID),
            (AnyGrainID, ContentContributorRoleID),
            (RootID, SuperuserRoleID),
            (RootID, EveryoneRoleID),
            (RootID, SchemaManagerRoleID),
            (SchemaContainerID, SchemaManagerRoleID),
            (SchemaContainerID, ContentContributorRoleID),
            (ContentContainerID, ContentContributorRoleID),
            (ContentContainerID, ContentConsumerRoleID),
            (FilesContainerID, ContentContributorRoleID),
            (FilesContainerID, ContentConsumerRoleID)
        };

        public static readonly IEnumerable<Type> GrainTierTypes = new[]
        {
            typeof(IFile), typeof(ITypeDef), typeof(IPropDef)
        };
    }
}
