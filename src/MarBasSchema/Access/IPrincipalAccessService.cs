namespace CraftedSolutions.MarBasSchema.Access
{
    public interface IPrincipalAccessService
    {
        bool VerifyRoleEntitlement(RoleEntitlement roleEntitlement, bool includeAllRoles = false);
    }
}
