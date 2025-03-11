namespace CraftedSolutions.MarBasSchema.Access
{
    public interface IAsyncPrincipalAccessService
    {
        Task<bool> VerifyRoleEntitlementAsync(RoleEntitlement roleEntitlement, bool includeAllRoles = false, CancellationToken cancellationToken = default);
    }
}
