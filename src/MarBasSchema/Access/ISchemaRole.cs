using MarBasCommon;

namespace MarBasSchema.Access
{
    [Flags]
    public enum RoleEntitlement: uint
    {
        None = 0x0,
        ReadAcl = 0x001,
        WriteAcl = 0x002,
        DeleteAcl = 0x004,
        ReadRoles = 0x010,
        WriteRoles = 0x020,
        DeleteRoles = 0x040,
        ExportSchema = 0x100,
        ImportSchema = 0x200,
        ModifySystemSettings = 0x1000,
        SkipPermissionCheck = 0x2000,
        DeleteBuiltInElements = 0x3000,
        Full = 0xffffffff
    }

    public interface ISchemaRole: IIdentifiable, INamed, IUpdateable
    {
        public RoleEntitlement Entitlement { get; set; }
    }
}
