using CraftedSolutions.MarBasBrokerSQLCommon;

namespace CraftedSolutions.MarBasBrokerSQLCommon.Access
{
    public class GrainAccessConfig<TDialect> where TDialect : ISQLDialect, new()
    {
        public const string SQLSelectAccessMask =
$@"SELECT _g.{GeneralEntityDefaults.FieldId}, (acl.{AclDefaults.FieldPermissionMask} & ~acl.{AclDefaults.FieldRestrictionMask}) AS {AclDefaults.FieldAccessMask}, acl.acl_type, RANK() OVER(PARTITION BY _g.{GeneralEntityDefaults.FieldId} ORDER BY acl.acl_type, acl.{AclDefaults.FieldRoleId}) AS acl_rank
	FROM mb_grain_base AS _g
	LEFT JOIN (
		SELECT * FROM {GrainAccessDefaults.DataSourceAclEffective}
 		WHERE {AclDefaults.FieldRoleId} = @{GrainAccessDefaults.ParamCurrentRole} OR {AclDefaults.FieldRoleId} = @{GrainAccessDefaults.ParamEveryoneRole} OR ({AclDefaults.FieldRoleId} IS NULL)
	) AS acl
	ON acl.{GeneralEntityDefaults.FieldGrainId} = _g.{GeneralEntityDefaults.FieldId} OR acl.acl_type >= 0xFFFFFFF0";

        public const string SQLJoinAclCheck =
$@"JOIN (
	{SQLSelectAccessMask}
) AS x
ON g.{GeneralEntityDefaults.FieldId} = x.{GeneralEntityDefaults.FieldId}
AND x.acl_rank = 1
AND (@{GrainAccessDefaults.ParamDesiredAccess} & x.{AclDefaults.FieldAccessMask}) > 0";

        public const string SQLAclCheck =
$@"SELECT x.{GeneralEntityDefaults.FieldId} FROM (
	{SQLSelectAccessMask}
) AS x
WHERE (@{GrainAccessDefaults.ParamDesiredAccess} & x.{AclDefaults.FieldAccessMask}) > 0
AND x.acl_rank = 1
AND ";
    }
}
