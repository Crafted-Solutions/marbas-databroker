﻿namespace MarBasCommon.DependencyInjection
{
    public interface IAsyncInitService
    {
        Task<bool> InitServiceAsync(CancellationToken cancellationToken = default);
    }
}
