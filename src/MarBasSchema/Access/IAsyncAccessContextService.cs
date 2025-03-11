namespace CraftedSolutions.MarBasSchema.Access
{
    public interface IAsyncAccessContextService
    {
        Task<ISchemaRole> GetContextPrimaryRoleAsync(CancellationToken cancellationToken = default);
        Task<IEnumerable<ISchemaRole>> GetContextRolesAsync(CancellationToken cancellationToken = default);
    }
}
