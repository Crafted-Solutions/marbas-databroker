﻿using System.Data.Common;
using System.Globalization;
using System.Net.Mime;
using MarBasBrokerSQLCommon.Grain;
using MarBasBrokerSQLCommon.GrainTier;
using MarBasCommon;
using MarBasSchema;
using MarBasSchema.Access;
using MarBasSchema.Broker;
using MarBasSchema.Grain;
using MarBasSchema.GrainTier;
using Microsoft.Extensions.Logging;

namespace MarBasBrokerSQLCommon.BrokerImpl
{
    public abstract class FileManagementBroker<TDialect>
        : GrainDefManagementBroker<TDialect>, IFileManagementBroker, IAsyncFileManagementBroker
        where TDialect : ISQLDialect, new()
    {
        #region Construction
        protected FileManagementBroker(IBrokerProfile profile, ILogger logger) : base(profile, logger)
        {
        }

        protected FileManagementBroker(IBrokerProfile profile, IBrokerContext context, IAsyncAccessService accessService, ILogger logger) : base(profile, context, accessService, logger)
        {
        }
        #endregion

        #region Public Intetrface
        public IGrainFile? GetGrainFile(Guid id, GrainFileContentAccess loadContent = GrainFileContentAccess.OnDemand, CultureInfo? culture = null)
        {
            return GetGrainFileAsync(id, loadContent, culture).Result;
        }

        public virtual async Task<IGrainFile?> GetGrainFileAsync(Guid id, GrainFileContentAccess loadContent = GrainFileContentAccess.OnDemand, CultureInfo? culture = null, CancellationToken cancellationToken = default)
        {
            CheckProfile();
            using (var conn = _profile.Connection)
            {
                await conn.OpenAsync(cancellationToken);
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = $"{(GrainFileContentAccess.Immediate == loadContent ? GrainFileConfig<TDialect>.SQLSelectFileByAclWithContent : GrainFileConfig<TDialect>.SQLSelectFileByAcl)}g.{AbstractDataAdapter.GetAdapterColumnName<GrainExtendedDataAdapter>(nameof(IGrainBase.Id))} = @{GeneralEntityDefaults.ParamId}";
                    _profile.ParameterFactory.AddParametersForGrainAclCheck(cmd.Parameters, (await _accessService.GetContextPrimaryRoleAsync(cancellationToken)).Id);
                    _profile.ParameterFactory.AddParametersForCultureLayer(cmd.Parameters, culture);
                    cmd.Parameters.Add(_profile.ParameterFactory.Create(GeneralEntityDefaults.ParamId, id));
                    using (var rs = await cmd.ExecuteReaderAsync(cancellationToken))
                    {
                        if (await rs.ReadAsync(cancellationToken))
                        {
                            return new GrainFile(CreateFileAdapter(rs, loadContent));
                        }
                    }
                }
            }
            return null;
        }

        public IGrainFile? CreateGrainFile(string name, string mimeType, Stream content, IIdentifiable? parent = null, long size = -1)
        {
            return CreateGrainFileAsync(name, mimeType, content, parent, size).Result;
        }

        public virtual async Task<IGrainFile?> CreateGrainFileAsync(string name, string mimeType, Stream content, IIdentifiable? parent = null, long size = -1, CancellationToken cancellationToken = default)
        {
            CheckProfile();
            IGrainFile? result = null;
            await WrapInTransaction(result, async (ta) =>
            {
                var grain = await CreateGrainInTA(name, parent ?? (Identifiable)SchemaDefaults.FilesContainerID, (Identifiable)SchemaDefaults.FileTypeDefID, ta, cancellationToken: cancellationToken);

                if (null != grain)
                {
                    var conn = ta.Connection!;

                    result = new GrainFile(grain)
                    {
                        MimeType = mimeType
                    };
                    await CreateGrainFileInTA(ta, grain, content, mimeType, size, cancellationToken);
                }
                return result;
            }, cancellationToken);
            return result;

        }

        public int StoreGrainFiles(IEnumerable<IGrainFile> files)
        {
            return StoreGrainFilesAsync(files).Result;
        }

        public virtual async Task<int> StoreGrainFilesAsync(IEnumerable<IGrainFile> files, CancellationToken cancellationToken = default)
        {
            var filesMod = files.Where(f => 0 < f.GetDirtyFields<IGrainFile>().Count);
            if (!filesMod.Any())
            {
                return -1;
            }
            CheckProfile();
            if (!await _accessService.VerfifyAccessAsync(files, GrainAccessFlag.Write, cancellationToken))
            {
                throw new SchemaAccessDeniedException(GrainAccessFlag.Write);
            }
            var result = 0;
            await WrapInTransaction(result, async (ta) =>
            {
                result = await StoreGrainFileTiersInTA(ta, filesMod, result, cancellationToken);
                return await StoreGrainsInTA(files.Where(g => 0 < g.GetDirtyFields<IGrainBase>().Count + g.GetDirtyFields<IGrainLocalized>().Count), result, ta, true, cancellationToken);
            }, cancellationToken);
            return result;
        }
        #endregion

        #region Helper Methods

        protected virtual async Task<object> CreateGrainFileInTA(DbTransaction ta, IIdentifiable grainBase, Stream content, string mimeType = MediaTypeNames.Application.Octet, long size = -1, CancellationToken cancellationToken = default)
        {
            var conn = ta.Connection!;

            object? result = null;
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = $"{GrainFileConfig<TDialect>.SQLInsertFile} ({GeneralEntityDefaults.FieldBaseId}, {MapFileColumn(nameof(IGrainFile.MimeType))}, {MapFileColumn(nameof(IGrainFile.Size))}, {MapFileColumn(nameof(IGrainFile.Content))}) VALUES (@{GeneralEntityDefaults.ParamId}, @{GrainFileDefaults.ParamMimeType}, @{GrainFileDefaults.ParamSize}, {EngineSpec<TDialect>.Dialect.NewBlobContent()}) {EngineSpec<TDialect>.Dialect.ReturnNewBlobID(GrainFileDefaults.DataSourceFile, MapFileColumn(nameof(IGrainFile.Content)), $"@{GeneralEntityDefaults.ParamId}")};";
                cmd.Parameters.Add(_profile.ParameterFactory.Create(GeneralEntityDefaults.ParamId, grainBase.Id));
                cmd.Parameters.Add(_profile.ParameterFactory.Create(GrainFileDefaults.ParamMimeType, mimeType));
                cmd.Parameters.Add(_profile.ParameterFactory.Create(GrainFileDefaults.ParamSize, 0 > size ? content.Length : size));

                result = await cmd.ExecuteScalarAsync(cancellationToken);
                if (null == result)
                {
                    throw new ApplicationException($"Failed to store file data for {grainBase.Id}");
                }
                await WriteFileBlobAsync(conn, content, result, cancellationToken);
            }
            return result;
        }

        protected virtual async Task<int> StoreGrainFileTiersInTA(DbTransaction ta, IEnumerable<IGrainFile> files, int result = 0, CancellationToken cancellationToken = default)
        {
            if (!files.Any())
            {
                return 0;
            }

            var conn = ta.Connection!;
            using (var cmd = conn.CreateCommand())
            {
                var contentField = nameof(IGrainFile.Content);
                var contentCol = MapFileColumn(contentField);

                foreach (var file in files)
                {
                    var hasContent = file.GetDirtyFields<IGrainFile>().Contains(contentField);
                    if (!hasContent || 1 < file.GetDirtyFields<IGrainFile>().Count)
                    {
                        file.GetDirtyFields<IGrainFile>().Remove(contentField);
                        cmd.Parameters.Clear();
                        cmd.CommandText = GrainFileConfig<TDialect>.SQLUpdateFile;
                        cmd.CommandText += _profile.ParameterFactory.PrepareDirtyFieldsUpdate<GrainFileInlineDataAdapter, IGrainFile>(cmd.Parameters, file);
                        cmd.CommandText += $" WHERE {GeneralEntityDefaults.FieldBaseId} = @{GeneralEntityDefaults.ParamId}";
                        cmd.Parameters.Add(_profile.ParameterFactory.Create(GeneralEntityDefaults.ParamId, file.Id));

                        var affected = await cmd.ExecuteNonQueryAsync(cancellationToken);
                        result += affected;
                        if (0 < affected)
                        {
                            file.GetDirtyFields<IGrainFile>().Clear();
                            if (hasContent)
                            {
                                file.GetDirtyFields<IGrainFile>().Add(contentField);
                            }
                        }
                    }
                    if (hasContent)
                    {
                        cmd.Parameters.Clear();
                        cmd.Parameters.Add(_profile.ParameterFactory.Create(GeneralEntityDefaults.ParamId, file.Id));

                        var sizeCol = MapFileColumn(nameof(IGrainFile.Size));

                        using (var content = file.Content)
                        {
                            if (null == content || 0 == file.Size)
                            {
                                result += await DeleteFileBlobAsync(conn, file, cancellationToken);

                                cmd.CommandText = $"{GrainFileConfig<TDialect>.SQLUpdateFile}{contentCol} = NULL, {sizeCol} = 0 WHERE {GeneralEntityDefaults.FieldBaseId} = @{GeneralEntityDefaults.ParamId}";
                                var affected = await cmd.ExecuteNonQueryAsync(cancellationToken);
                                result += affected;
                                if (0 < affected)
                                {
                                    file.GetDirtyFields<IGrainFile>().Clear();
                                }
                            }
                            else
                            {
                                cmd.CommandText = $"{GrainFileConfig<TDialect>.SQLUpdateFile}";
                                if (EngineSpec<TDialect>.Dialect.BlobUpdateRequiresReset)
                                {
                                    cmd.CommandText += $"{contentCol} = {EngineSpec<TDialect>.Dialect.NewBlobContent()}, ";
                                }
                                cmd.CommandText += $"{sizeCol} = @{GrainFileDefaults.ParamSize} WHERE {GeneralEntityDefaults.FieldBaseId} = @{GeneralEntityDefaults.ParamId} {EngineSpec<TDialect>.Dialect.ReturnExistingBlobID(GrainFileDefaults.DataSourceFile, GrainFileDefaults.FieldContent, $"@{GeneralEntityDefaults.ParamId}")}";
                                cmd.Parameters.Add(_profile.ParameterFactory.Create(GrainFileDefaults.ParamSize, file.Size));

                                var id = await cmd.ExecuteScalarAsync(cancellationToken);
                                if (null == id)
                                {
                                    if (_logger.IsEnabled(LogLevel.Error))
                                    {
                                        _logger.LogError("Failed to find record for file {fileId}", file.Id);
                                    }
                                }
                                else
                                {
                                    await WriteFileBlobAsync(conn, content.Stream, id, cancellationToken);
                                    file.GetDirtyFields<IGrainFile>().Clear();
                                    result += 1;
                                }
                            }
                        }

                    }
                }
            }
            return result;
        }

        protected abstract Task WriteFileBlobAsync(DbConnection connection, Stream content, object blobId, CancellationToken cancellationToken = default);

        protected virtual IGrainFile CreateFileAdapter(DbDataReader reader, GrainFileContentAccess loadContent = GrainFileContentAccess.OnDemand)
        {
            return new GrainFileInlineDataAdapter(reader, loadContent);
        }

        protected virtual Task<int> DeleteFileBlobAsync(DbConnection connection, IGrainFile file, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(0);
        }

        protected static string MapFileColumn(string fieldName)
        {
            return AbstractDataAdapter.GetAdapterColumnName<GrainFileInlineDataAdapter>(fieldName);
        }
        #endregion
    }
}
