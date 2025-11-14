//#if DEBUG
//#define PROFILE_PKG_EXPORT
//#endif

using CraftedSolutions.MarBasCommon;
using CraftedSolutions.MarBasCommon.Job;
using CraftedSolutions.MarBasSchema.Broker;
using CraftedSolutions.MarBasSchema.Grain;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.IO.Compression;
using System.Text.Json;


namespace CraftedSolutions.MarBasSchema.Transport
{
    internal class TempDirectory : IDisposable
    {
        private bool _disposed;
        private readonly DirectoryInfo _dirInfo;
        private readonly JsonSerializerOptions? _serializerOptions;

        public TempDirectory(string? prefix = default, JsonSerializerOptions? serializerOptions = null)
        {
            _dirInfo = Directory.CreateTempSubdirectory(prefix);
            _serializerOptions = serializerOptions;
        }

        public TempDirectory(DirectoryInfo existingDirectory, JsonSerializerOptions? serializerOptions = null)
        {
            _dirInfo = existingDirectory;
            _serializerOptions = serializerOptions;
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                if (_dirInfo.Exists)
                {
                    _dirInfo.Delete(true);
                }
                _disposed = true;
            }
            GC.SuppressFinalize(this);
        }

        public DirectoryInfo Info => _dirInfo;

        public bool ContainsFile(string name)
        {
            return File.Exists(Path.Combine(_dirInfo.FullName, name));
        }

        public async Task<T> ReadEntry<T>(string name, CancellationToken cancellationToken = default)
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            using var entryStream = File.OpenRead(Path.Combine(_dirInfo.FullName, name));
            var result = await JsonSerializer.DeserializeAsync<T>(entryStream, _serializerOptions, cancellationToken);
            if (null == result)
            {
                throw new ApplicationException($"Failed to read {name}");
            }
            return result;
        }

        public async Task<IGrainTransportable> ReadGrain(Guid id, CancellationToken cancellationToken = default)
        {
            return await ReadEntry<GrainTransportable>(id.MakeSerializedFileName(GrainTransportableExtension.GrainQualifier), cancellationToken);
        }

        public async Task<bool> WriteEntry<T>(T entry, string name, CancellationToken cancellationToken = default)
        {
            if (!ContainsFile(name))
            {
                ObjectDisposedException.ThrowIf(_disposed, this);
                using var entryStream = File.OpenWrite(Path.Combine(_dirInfo.FullName, name));
                await JsonSerializer.SerializeAsync(entryStream, entry, _serializerOptions, cancellationToken);
                return true;
            }
            return false;
        }

        public async Task<bool> WriteGrain(IGrainTransportable grain, CancellationToken cancellationToken = default)
        {
            return await WriteEntry(grain, grain.MakeSerializedFileName(), cancellationToken);
        }
    }

    internal class PackageManifest
    {
        public Guid InstanceId { get; set; }
        public required Version SchemaVersion { get; set; }
        public DateTime Created { get; set; } = DateTime.UtcNow;
        public IEnumerable<Guid> AnchorGrainIds { get; set; } = [];
    }

    public class SchemaPackager(IAsyncSchemaBroker broker, IBackgroundWorkQueue taskQueue, IBackgroundJobManager jobManager, JsonSerializerOptions serializerOptions, ILogger<SchemaPackager> logger)
        : ISchemaPackager, IAsyncSchemaPackager
    {
        public const string ManifestName = "manifest.json";
        public const string PackagePrefix = "marbas-export-";
        public const string GrainChildrenNameSuffix = $"{GrainTransportableExtension.FileNameFieldSeparator}c.json";

        #region Variables
        private readonly IAsyncSchemaBroker _broker = broker;
        private readonly IBackgroundWorkQueue _taskQueue = taskQueue;
        private readonly IBackgroundJobManager _jobManager = jobManager;
#if DEBUG
        private readonly JsonSerializerOptions _serializerOptions = new (serializerOptions) { WriteIndented = true };
#else
        private readonly JsonSerializerOptions _serializerOptions = serializerOptions;
#endif
        private readonly ILogger<SchemaPackager> _logger = logger;
#endregion

        #region Public Interface
/*
{
    "items": {
        "c1fd9974-1204-4c29-a721-700405983d92": {},
        "f5a20495-400d-4584-b75f-211500026b0b": {}
    }
}
{
    "namePrefix": "madare-export-",
    "items": {
        "babe6743-fd36-4856-9ef8-7b0768ea8cc7": { "priority": 0, "typeDefTraversal": "Immediate", "childrenTraversal": "Indefinite" },
        "61620f34-cf29-44ce-835e-0206f3e8a6e2": { "priority": 1, "typeDefTraversal": "Immediate", "childrenTraversal": "Indefinite" }
    }
}

{
    "items": {
        "b450ab76-13de-4557-b914-3c188178614f": { "typeDefTraversal": "Immediate", "childrenTraversal": "Indefinite" }
    }
}
*/

        public Stream ExportPackage(IDictionary<Guid, IGrainPackagingOptions> packageDefinition)
        {
            return ExportPackageAsync(packageDefinition).Result;
        }

        public async Task<Stream> ExportPackageAsync(IDictionary<Guid, IGrainPackagingOptions> packageDefinition, CancellationToken cancellationToken = default)
        {
#if PROFILE_PKG_EXPORT
            var start = DateTime.UtcNow.Ticks;
#endif
            var topGrains = await _broker.ExportGrainsAsync(packageDefinition.Keys.Select(x => (Identifiable)x), cancellationToken);
            if (!topGrains.Any())
            {
                throw new ArgumentOutOfRangeException($"None of the {string.Join(", ", packageDefinition.Keys)} is exportable");
            }
            if (_logger.IsEnabled(LogLevel.Debug))
            {
                _logger.LogDebug("Exporting {grains}", string.Join(", ", packageDefinition.Keys));
            }

            var orderedGrains = packageDefinition.OrderBy(x => x.Value.Priority).Select(x => topGrains.SingleOrDefault(y => y.Id == x.Key));
            var manifest = new PackageManifest()
            {
                SchemaVersion = _broker.Profile.Version,
                InstanceId = _broker.Profile.InstanceId,
                AnchorGrainIds = orderedGrains.Where(x => null != x).Select(x => x!.Id)
            };

            var result = new MemoryStream();

#if PROFILE_PKG_EXPORT
            var preparing = DateTime.UtcNow.Ticks - start;
            var caching = 0L;
#endif
            using (var tempDir = new TempDirectory(PackagePrefix, _serializerOptions))
            {
                var grainCount = 0;
                foreach (var grain in orderedGrains)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    if (null == grain)
                    {
                        continue;
                    }
                    var options = packageDefinition[grain.Id];
                    if (ReferenceDepth.None != options.ParentTraversal)
                    {
                        var path = await _broker.GetGrainAncestorsAsync(grain, cancellationToken: cancellationToken);
                        var parents = await _broker.ExportGrainsAsync(ReferenceDepth.Immediate == options.ParentTraversal ? [path.First()] : path, cancellationToken);

                        foreach (var parent in parents)
                        {
                            cancellationToken.ThrowIfCancellationRequested();
                            grainCount += await TraverseAndCacheGrain(parent, tempDir, options, 0, cancellationToken);
                        }
                    }
                    grainCount += await TraverseAndCacheGrain(grain, tempDir, options, 0, cancellationToken);
                }

#if PROFILE_PKG_EXPORT
                caching = DateTime.UtcNow.Ticks - preparing - start;
#endif

                if (_logger.IsEnabled(LogLevel.Debug))
                {
                    _logger.LogDebug("Packaging {grainCount} grains related to {topIds}", grainCount, string.Join(", ", packageDefinition.Keys));
                }
                cancellationToken.ThrowIfCancellationRequested();
                result.Capacity = 0x1000 + 0x200 * grainCount;

                await Task.Run(async () =>
                {
                    using (var zip = new ZipArchive(result, ZipArchiveMode.Create, true))
                    {
                        foreach (var file in tempDir.Info.GetFiles())
                        {
                            cancellationToken.ThrowIfCancellationRequested();
                            zip.CreateEntryFromFile(file.FullName, file.Name);
                        }
                        await AddEntryToPackage(manifest, ManifestName, zip, cancellationToken);
                    }
                }, cancellationToken);

            }
            result.Seek(0, SeekOrigin.Begin);

#if PROFILE_PKG_EXPORT
            Console.WriteLine($"PERF: PackageOut preparing = {(new TimeSpan(preparing).TotalMilliseconds)}, caching = {(new TimeSpan(caching).TotalMilliseconds)}, packaging = {(new TimeSpan(DateTime.UtcNow.Ticks - caching - preparing - start)).TotalMilliseconds}");
#endif

            return result;
        }

        public IBackgroundJob SchedulePackageImport(Stream packageStream, DuplicatesHandlingStrategy duplicatesHandling = DuplicatesHandlingStrategy.MergeSkipNewer, MissingDependencyHandlingStrategy missingDependencyHandling = MissingDependencyHandlingStrategy.CreatePlaceholder)
        {
            return SchedulePackageImportAsync(packageStream, duplicatesHandling, missingDependencyHandling).Result;
        }

        public async Task<IBackgroundJob> SchedulePackageImportAsync(Stream packageStream, DuplicatesHandlingStrategy duplicatesHandling = DuplicatesHandlingStrategy.MergeSkipNewer, MissingDependencyHandlingStrategy missingDependencyHandling = MissingDependencyHandlingStrategy.CreatePlaceholder, CancellationToken cancellationToken = default)
        {
            var job = _jobManager.EmplaceJob("PackageImport");
            var tempDir = new TempDirectory($"marbas-import-{job.Id}-", _serializerOptions);
            job.RegisterForDispose(tempDir);
            job.Stage = "Caching";
            try
            {
                using (var zip = new ZipArchive(packageStream))
                {
                    zip.ExtractToDirectory(tempDir.Info.FullName, true);
                }
                if (!tempDir.ContainsFile(ManifestName))
                {
                    throw new ArgumentException($"{ManifestName} is missing in package");
                }
            }
            catch
            {
                _jobManager.RemoveJob(job.Id, true);
                throw;
            }
            await _taskQueue.QueueWorkItemAsync(async (token) =>
            {
                using (tempDir)
                {
                    var jobCtx = new BackgroundJob.Context(job, token);

                    try
                    {
                        jobCtx.Status = BackgroundJobStatus.Running;
                        if (_logger.IsEnabled(LogLevel.Debug))
                        {
                            _logger.LogDebug("Starting {name} job ({id})", job.Name, job.Id);
                        }

                        var processor = new ImportCacheProcessor(_broker, tempDir, duplicatesHandling, missingDependencyHandling);
                        jobCtx.Result = await processor.Invoke(jobCtx);
                        jobCtx.Status = BackgroundJobStatus.Complete;
                        jobCtx.Stage = "Ready";

                        if (_logger.IsEnabled(LogLevel.Debug))
                        {
                            _logger.LogDebug("{name} job ({id}) is complete", job.Name, job.Id);
                        }
                    }
                    catch (OperationCanceledException)
                    {
                        jobCtx.Status = BackgroundJobStatus.Cancelled;
                    }
                    catch (Exception e)
                    {
                        if (_logger.IsEnabled(LogLevel.Error))
                        {
                            _logger.LogError(e, "Error processing {name} job ({id})", job.Name, job.Id);
                        }
                        jobCtx.Status = BackgroundJobStatus.Error;
                        jobCtx.Result = e.Message;
                    }
                }

            });

            return job;
        }
        #endregion

        #region Helpers

        private class ImportCacheProcessor(IAsyncSchemaBroker broker, TempDirectory cacheDir
            , DuplicatesHandlingStrategy duplicatesHandling, MissingDependencyHandlingStrategy missingDependencyHandling)
        {
            private readonly IAsyncSchemaBroker _broker = broker;
            private readonly TempDirectory _cacheDir = cacheDir;
            private readonly DuplicatesHandlingStrategy _duplicatesHandling = duplicatesHandling;
            private readonly MissingDependencyHandlingStrategy _missingDependencyHandling = missingDependencyHandling;
            private readonly ConcurrentDictionary<Guid, byte> _processedGrains = [];
            private readonly DateTime _startTime = DateTime.UtcNow;

            private readonly IGrain?[] _placeholderRoots = new IGrain?[3];

            public async Task<IGrainImportResults> Invoke(IBackgroudJobContext jobContext)
            {
                jobContext.Stage = "Import";
                var manifest = await _cacheDir.ReadEntry<PackageManifest>(ManifestName, jobContext.CancellationToken);
                return await ProcessGrains(manifest.AnchorGrainIds, jobContext);
            }

            private async Task<IGrainImportResults> ProcessGrains(IEnumerable<Guid> ids, IBackgroudJobContext jobContext)
            {
                var result = new GrainImportResults();
                var cancellationToken = jobContext.CancellationToken;
                foreach (var id in ids)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    if (_processedGrains.ContainsKey(id) || !_cacheDir.ContainsFile(id.MakeSerializedFileName(GrainTransportableExtension.GrainQualifier)))
                    {
                        continue;
                    }
                    var grain = await _cacheDir.ReadGrain(id, cancellationToken);
                    var stage = $"Import-{grain.Id}-{grain.Name}";
                    jobContext.Stage = stage;

                    var dependencies = grain.GetDependencies(GrainDependencyFlags.IncludeLinks | GrainDependencyFlags.IncludeTypeDefs | GrainDependencyFlags.IncludeParent, true).ToList();
                    var missing = (await _broker.VerifyGrainsExistAsync(dependencies.Where(x => !_processedGrains.ContainsKey(x.Id) && !_cacheDir.ContainsFile(x.MakeSerializedFileName())).Select(x => x.Id), cancellationToken))
                        .Where(x => !x.Value).Select(x => x.Key).Concat(dependencies.Where(x => _processedGrains.ContainsKey(x.Id) && 0 == _processedGrains[x.Id]).Select(x => x.Id));
                    if (missing.Any())
                    {
                        if (MissingDependencyHandlingStrategy.Abort == _missingDependencyHandling)
                        {
                            throw new ApplicationException($"Import aborted due to grain {id} missing dependencies {string.Join(", ", missing)}");
                        }
                        if (MissingDependencyHandlingStrategy.WarnAndContinue == _missingDependencyHandling)
                        {
                            result.IgnoredCount = 1;
                            result.AddFeedback(new BrokerOperationFeedback($"Grain {id} skipped due to missing dependencies {string.Join(", ", missing)}", "PackageImport", 404, LogLevel.Warning, id));
                            _processedGrains[id] = 0;
                            continue;
                        }
                        else
                        {
                            var placeholders = new List<IGrainTransportable>();
                            foreach (var missingId in missing)
                            {
                                var missingGrain = (IGrain?)dependencies.FirstOrDefault(x => x.Id == missingId);
                                if (null == missingGrain)
                                {
                                    continue;
                                }
                                var placeholder = new GrainTransportable(missingGrain)
                                {
                                    ParentId = (await GetPlaceholderParent(missingGrain, grain, cancellationToken)).Id,
                                    Name = $"{missingId:D}{(null == missingGrain.Name ? string.Empty : $"_{missingGrain.Name}")}"
                                };
                                if (!string.IsNullOrEmpty(placeholder.Path))
                                {
                                    placeholder.Localized = new Dictionary<string, IGrainLocalizedLayer>() { { SchemaDefaults.Culture.IetfLanguageTag, new GrainLocalizedLayer() { Label = placeholder.Path } } };
                                }
                                placeholders.Add(placeholder);
                                _processedGrains[missingId] = 1;
                            }
                            result = GrainImportResults.Merge(result, await _broker.ImportGrainsAsync(placeholders, duplicatesHandling: _duplicatesHandling, cancellationToken: cancellationToken));
                        }
                    }

                    if (!cancellationToken.IsCancellationRequested)
                    {
                        result = GrainImportResults.Merge(result, await ProcessGrains(dependencies.Select(x => x.Id), jobContext));

                        jobContext.Stage = stage;
                        var currentResult = await _broker.ImportGrainsAsync([grain], duplicatesHandling: _duplicatesHandling, cancellationToken: cancellationToken);
                        result = GrainImportResults.Merge(result, currentResult);
                        _processedGrains[id] = 1;
                    }

                    var childrenEntry = grain.MakeSerializedFileName(extension: GrainChildrenNameSuffix);
                    if (!cancellationToken.IsCancellationRequested && _cacheDir.ContainsFile(childrenEntry))
                    {
                        var children = await _cacheDir.ReadEntry<IEnumerable<Guid>>(childrenEntry, cancellationToken);
                        result = GrainImportResults.Merge(result, await ProcessGrains(children, jobContext));
                    }
                }
                return result;
            }

            private async Task<IGrain> GetPlaceholderParent(IGrain missingGrain, IGrainTransportable dependentGrain, CancellationToken cancellationToken)
            {
                var phIdx = 2;
                if (null == missingGrain.TypeDefId
                    || (dependentGrain.ParentId == missingGrain.Id && (null == dependentGrain.TypeDefId || dependentGrain.Tier is IGrainTierPropDef || true == dependentGrain.Path?.StartsWith(SchemaDefaults.SchemaContainerPath))))
                {
                    phIdx = 0;
                }
                else if (SchemaDefaults.FileTypeDefID == missingGrain.TypeDefId
                    || (dependentGrain.ParentId == missingGrain.Id && (dependentGrain.Tier is IGrainTierFile || true == dependentGrain.Path?.StartsWith(SchemaDefaults.FileContainerPath))))
                {
                    phIdx = 1;
                }
                var result = _placeholderRoots[phIdx];
                if (null == result)
                {
                    var parentRoot = phIdx switch
                    {
                        0 => SchemaDefaults.UserSchemaContainerID,
                        1 => SchemaDefaults.FilesContainerID,
                        _ => SchemaDefaults.ContentContainerID
                    };
                    var name = $"_Import_Orphans_{_startTime:yyyyMMddHHmmssfff}";
                    _placeholderRoots[phIdx] = result = await _broker.CreateGrainAsync(name, (Identifiable)parentRoot, (Identifiable)SchemaDefaults.ContainerTypeDefID, cancellationToken);
                    if (null == result)
                    {
                        throw new ApplicationException($"Failed to create placeholder container for {missingGrain.Id} under {parentRoot}");
                    }
                }
                return result;
            }
        }

        private async Task<int> TraverseAndCacheGrain(IGrainTransportable grain, TempDirectory cacheDir, IGrainPackagingOptions grainOptions, int currentDepth, CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested || !await cacheDir.WriteGrain(grain, cancellationToken))
            {
                return 0;
            }

            bool CheckDepth(ReferenceDepth targetDepth)
            {
                return ReferenceDepth.Indefinite == targetDepth
                    || (0 == currentDepth && ReferenceDepth.None != targetDepth)
                    || (1 == currentDepth && ReferenceDepth.Immediate == targetDepth);
            }

            bool CheckGrainCached(Guid id)
            {
                return cacheDir.ContainsFile(id.MakeSerializedFileName(GrainTransportableExtension.GrainQualifier));
            }

#if PROFILE_PKG_EXPORT
            var start = DateTime.UtcNow.Ticks;
#endif
            var result = 1;
            if (0 < (grainOptions.LinksTraversal | grainOptions.TypeDefTraversal))
            {
                var depsFlags = GrainDependencyFlags.IncludeNone;
                if (CheckDepth(grainOptions.LinksTraversal))
                {
                    depsFlags |= GrainDependencyFlags.IncludeLinks;
                }
                if (CheckDepth(grainOptions.TypeDefTraversal))
                {
                    depsFlags |= GrainDependencyFlags.IncludeTypeDefs;
                }
                var dependencies = await _broker.ExportGrainsAsync(grain.GetDependencies(depsFlags).Where(x => !CheckGrainCached(x.Id)), cancellationToken);
                foreach (var dep in dependencies)
                {
                    result += await TraverseAndCacheGrain(dep, cacheDir, grainOptions, grain.TypeDefId == dep.Id ? 0 : currentDepth + 1, cancellationToken);
                }
            }
#if PROFILE_PKG_EXPORT
            var basetraverse = DateTime.UtcNow.Ticks - start;
#endif
            if (null == grain.TypeDefId && CheckDepth(grainOptions.TypeDefTraversal))
            {
                grainOptions = grainOptions.Clone();
                grainOptions.ChildrenTraversal = ReferenceDepth.Indefinite;
            }
            if (!cancellationToken.IsCancellationRequested && CheckDepth(grainOptions.ChildrenTraversal))
            {
#if PROFILE_PKG_EXPORT
                var listing = basetraverse;
#endif
                var childGrains = (await _broker.ListGrainsAsync(grain, ReferenceDepth.Indefinite == grainOptions.ChildrenTraversal, cancellationToken: cancellationToken));
#if PROFILE_PKG_EXPORT
                    listing = DateTime.UtcNow.Ticks - basetraverse - start;
#endif
                var children = await _broker.ExportGrainsAsync(childGrains.Where(x => !CheckGrainCached(x.Id)), cancellationToken);
#if PROFILE_PKG_EXPORT
                Console.WriteLine($"PERF: TraverseAndCacheGrain {grain.Id} listing = {(new TimeSpan(listing)).TotalMilliseconds}, exporting = {(new TimeSpan(DateTime.UtcNow.Ticks - basetraverse - listing - start)).TotalMilliseconds}");
#endif
                if (ReferenceDepth.Indefinite == grainOptions.ChildrenTraversal)
                {
                    grainOptions = grainOptions.Clone();
                    grainOptions.ChildrenTraversal = ReferenceDepth.None;
                }

                foreach (var child in children)
                {
                    result += await TraverseAndCacheGrain(child, cacheDir, grainOptions, currentDepth + 1, cancellationToken);
                }

                await Parallel.ForEachAsync(childGrains.GroupBy(x => (Guid)x.ParentId!), new ParallelOptions { MaxDegreeOfParallelism = 3, CancellationToken = cancellationToken }, async (parentGroup, token) =>
                {
                    await cacheDir.WriteEntry(parentGroup.Select(x => x.Id).OrderBy(x => x)
                        , parentGroup.Key.MakeSerializedFileName(GrainTransportableExtension.GrainQualifier, extension: GrainChildrenNameSuffix), token);

                });
            }
#if PROFILE_PKG_EXPORT
            Console.WriteLine($"PERF: TraverseAndCacheGrain {grain.Id} basis = {(new TimeSpan(basetraverse)).TotalMilliseconds}, children = {(new TimeSpan(DateTime.UtcNow.Ticks - basetraverse - start)).TotalMilliseconds}");
#endif
            return result;
        }

        private async Task AddEntryToPackage(object data, string name, ZipArchive zip, CancellationToken cancellationToken)
        {
            var entry = zip.CreateEntry(name);
            using var entryStream = entry.Open();
            await JsonSerializer.SerializeAsync(entryStream, data, _serializerOptions, cancellationToken);
        }
    }
    #endregion
}
