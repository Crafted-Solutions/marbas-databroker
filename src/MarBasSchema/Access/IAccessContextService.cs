﻿namespace MarBasSchema.Access
{
    public interface IAccessContextService
    {
        ISchemaRole GetContextPrimaryRole();
        IEnumerable<ISchemaRole> GetContextRoles();
    }
}
