namespace CraftedSolutions.MarBasBrokerSQLCommon.Access
{
    public static class AclDefaults
    {
        public const string DataSourceAcl = "mb_grain_acl";
        public const string DataSourceAclEffective = "mb_grain_acl_effective";

        public const string FieldRoleId = "role_id";
        public const string FieldPermissionMask = "permission_mask";
        public const string FieldRestrictionMask = "restriction_mask";
        public const string FieldSourceGrain = "acl_source";
        public const string FieldAccessMask = "access_mask";

        public const string ParamRoleId = "roleId";
        public const string ParamGrainId = "grainId";
        public const string ParamPermissionMask = "permissionMask";
        public const string ParamRestrictionMask = "restrictionMask";
        public const string ParamInherit = "inherit";
    }
}
