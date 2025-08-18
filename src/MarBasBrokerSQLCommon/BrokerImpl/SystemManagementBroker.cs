using System.Data.Common;
using System.Globalization;
using CraftedSolutions.MarBasBrokerSQLCommon;
using CraftedSolutions.MarBasBrokerSQLCommon.Sys;
using CraftedSolutions.MarBasSchema;
using CraftedSolutions.MarBasSchema.Access;
using CraftedSolutions.MarBasSchema.Broker;
using CraftedSolutions.MarBasSchema.Sys;
using Microsoft.Extensions.Logging;

namespace CraftedSolutions.MarBasBrokerSQLCommon.BrokerImpl
{
    public abstract class SystemManagementBroker<TDialect>
        : BaseSchemaBroker<TDialect>, ISystemLanguageBroker, IAsyncSystemLanguageBroker
         where TDialect : ISQLDialect, new()
    {
        protected SystemManagementBroker(IBrokerProfile profile, ILogger logger) : base(profile, logger)
        {
        }

        protected SystemManagementBroker(IBrokerProfile profile, IBrokerContext context, IAsyncAccessService accessService, ILogger logger) : base(profile, context, accessService, logger)
        {
        }

        public ISystemLanguage? GetSystemLanguage(CultureInfo culture)
        {
            return GetSystemLanguageAsync(culture).Result;
        }

        public async Task<ISystemLanguage?> GetSystemLanguageAsync(CultureInfo culture, CancellationToken cancellationToken = default)
        {
            await CheckProfile(cancellationToken);
            return await ExecuteOnConnection(null, async (cmd) =>
            {
                using (cmd)
                {
                    cmd.CommandText = $"{SystemLanguageConfig<TDialect>.SQLSelectLang}{AbstractDataAdapter.GetAdapterColumnName<SystemLanguageDataAdapter>(nameof(ISystemLanguage.IsoCode))} = @{SystemLanguageDefaults.ParamIsoCode}";
                    cmd.Parameters.Add(_profile.ParameterFactory.Create(SystemLanguageDefaults.ParamIsoCode, culture.IetfLanguageTag));
                    using (var rs = await cmd.ExecuteReaderAsync(cancellationToken))
                    {
                        if (await rs.ReadAsync(cancellationToken))
                        {
                            return new SystemLanguage(new SystemLanguageDataAdapter(rs));
                        }
                    }
                }
                return null;
            }, cancellationToken);
        }

        public IEnumerable<ISystemLanguage> ListSystemLanguages(IEnumerable<CultureInfo>? cultures = null)
        {
            return ListSystemLanguagesAsync(cultures).Result;
        }

        public async Task<IEnumerable<ISystemLanguage>> ListSystemLanguagesAsync(IEnumerable<CultureInfo>? cultures = null, CancellationToken cancellationToken = default)
        {
            await CheckProfile(cancellationToken);
            var result = new List<ISystemLanguage>();
            return await ExecuteOnConnection(result, async (cmd) =>
            {
                var codeCol = AbstractDataAdapter.GetAdapterColumnName<SystemLanguageDataAdapter>(nameof(ISystemLanguage.IsoCode));

                cmd.CommandText = $"{SystemLanguageConfig<TDialect>.SQLSelectLang}";
                if (null != cultures)
                {
                    var i = 0;
                    var clause = cultures.Aggregate(string.Empty, (aggr, elm) =>
                    {
                        var paramName = $"{SystemLanguageDefaults.ParamIsoCode}{i++}";
                        cmd.Parameters.Add(_profile.ParameterFactory.Create(paramName, elm.IetfLanguageTag));
                        return 0 == aggr.Length ? $"@{paramName}" : $"{aggr}, @{paramName}";
                    });
                    if (0 < clause.Length)
                    {
                        cmd.CommandText += $"{codeCol} IN ({clause})";
                    }
                }
                if (0 == cmd.Parameters.Count)
                {
                    cmd.CommandText += "(1 = 1)";
                }

                cmd.CommandText += $@" ORDER BY
	CASE
		WHEN {codeCol} = @{GeneralEntityDefaults.ParamLangDefault} OR {codeCol} LIKE @{GeneralEntityDefaults.ParamLangPrefix} THEN 1
		ELSE 2
	END, {codeCol}";

                cmd.Parameters.Add(_profile.ParameterFactory.Create(GeneralEntityDefaults.ParamLangDefault, SchemaDefaults.Culture.TwoLetterISOLanguageName));
                cmd.Parameters.Add(_profile.ParameterFactory.Create(GeneralEntityDefaults.ParamLangPrefix, $"{SchemaDefaults.Culture.TwoLetterISOLanguageName}-%"));

                using (var rs = await cmd.ExecuteReaderAsync(cancellationToken))
                {
                    while (await rs.ReadAsync(cancellationToken))
                    {
                        result.Add(new SystemLanguage(new SystemLanguageDataAdapter(rs)));
                    }

                }
                return result;
            }, cancellationToken);
        }

        public ISystemLanguage? CreateSystemLanguage(CultureInfo culture)
        {
            return CreateSystemLanguageAsync(culture).Result;
        }

        public async Task<ISystemLanguage?> CreateSystemLanguageAsync(CultureInfo culture, CancellationToken cancellationToken = default)
        {
            await CheckProfile(cancellationToken);
            if (!await _accessService.VerifyRoleEntitlementAsync(RoleEntitlement.ModifySystemSettings, cancellationToken: cancellationToken))
            {
                throw new UnauthorizedAccessException("Not entitled to add language");
            }
            return await WrapInTransaction(null, async (ta) =>
            {
                return await CreateSystemLanguageInTA(ta, culture, cancellationToken: cancellationToken);
            }, cancellationToken);
        }

        public int StoreSystemLanguages(IEnumerable<ISystemLanguage> languages)
        {
            return StoreSystemLanguagesAsync(languages).Result;
        }

        public async Task<int> StoreSystemLanguagesAsync(IEnumerable<ISystemLanguage> languages, CancellationToken cancellationToken = default)
        {
            await CheckProfile(cancellationToken);
            var langMod = languages.Where(a => 0 < a.GetDirtyFields<ISystemLanguage>().Count);
            if (!langMod.Any())
            {
                return -1;
            }
            var result = 0;
            result = await WrapInTransaction(result, async (ta) =>
            {
                using (var cmd = ta.Connection!.CreateCommand())
                {
                    var codeCol = AbstractDataAdapter.GetAdapterColumnName<SystemLanguageDataAdapter>(nameof(ISystemLanguage.IsoCode));
                    foreach (var lang in langMod)
                    {
                        cmd.Parameters.Clear();
                        cmd.CommandText = SystemLanguageConfig<TDialect>.SQLUpdateLang;

                        cmd.CommandText += _profile.ParameterFactory.PrepareDirtyFieldsUpdate<SystemLanguageDataAdapter, ISystemLanguage>(cmd.Parameters, lang);
                        cmd.CommandText += $" WHERE {codeCol} = @{SystemLanguageDefaults.ParamIsoCode}";

                        cmd.Parameters.Add(_profile.ParameterFactory.Create(SystemLanguageDefaults.ParamIsoCode, lang.IsoCode));

                        var affected = await cmd.ExecuteNonQueryAsync(cancellationToken);
                        result += affected;
                        if (0 < affected)
                        {
                            lang.GetDirtyFields<ISystemLanguage>().Clear();
                        }
                    }
                }
                return result;
            }, cancellationToken);
            return result;
        }

        public int DeleteSystemLanguages(IEnumerable<ISystemLanguageRef> languages)
        {
            return DeleteSystemLanguagesAsync(languages).Result;
        }

        public async Task<int> DeleteSystemLanguagesAsync(IEnumerable<ISystemLanguageRef> languages, CancellationToken cancellationToken = default)
        {
            await CheckProfile(cancellationToken);
            if (!await _accessService.VerifyRoleEntitlementAsync(RoleEntitlement.ModifySystemSettings, cancellationToken: cancellationToken))
            {
                throw new UnauthorizedAccessException("Not entitled to delete language");
            }
            var result = 0;
            result = await WrapInTransaction(result, async (ta) =>
            {
                using (var cmd = ta.Connection!.CreateCommand())
                {
                    cmd.CommandText = $"{SystemLanguageConfig<TDialect>.SQLDeleteLang}{AbstractDataAdapter.GetAdapterColumnName<SystemLanguageDataAdapter>(nameof(ISystemLanguage.IsoCode))} = @{SystemLanguageDefaults.ParamIsoCode}";
                    cmd.Parameters.Add(_profile.ParameterFactory.Create(SystemLanguageDefaults.ParamIsoCode, string.Empty));

                    foreach (var lang in languages)
                    {
                        _profile.ParameterFactory.Update(cmd.Parameters[0], lang.IsoCode);

                        result += await cmd.ExecuteNonQueryAsync(cancellationToken);
                    }
                }
                return result;
            }, cancellationToken);
            return result;
        }

        public IEnumerable<bool> CheckSystemLanguagesExist(IEnumerable<string> languages)
        {
            return CheckSystemLanguagesExistAsync(languages).Result;
        }

        public async Task<IEnumerable<bool>> CheckSystemLanguagesExistAsync(IEnumerable<string> languages, CancellationToken cancellationToken = default)
        {
            var result = new bool[languages.Count()];
            if (!languages.Any())
            {
                return result;
            }
            await CheckProfile(cancellationToken);
            return await ExecuteOnConnection(result, async (cmd) =>
            {
                using (cmd)
                {
                    var langOrder = languages.ToList();
                    var vals = languages.Select((x, index) =>
                    {
                        var paramName = $"{SystemLanguageDefaults.ParamIsoCode}{index}";
                        cmd.Parameters.Add(_profile.ParameterFactory.Create(paramName, x));
                        return paramName;
                    });
                    var codeField = AbstractDataAdapter.GetAdapterColumnName<SystemLanguageDataAdapter>(nameof(ISystemLanguage.IsoCode));
                    cmd.CommandText = $"SELECT {codeField} FROM {SystemLanguageDefaults.DataSourceLang} WHERE {codeField} IN (@{string.Join(",@", vals)})";

                    using (var rs = await cmd.ExecuteReaderAsync(cancellationToken))
                    {
                        while (await rs.ReadAsync(cancellationToken))
                        {
                            result[langOrder.IndexOf(rs.GetString(0))] = true;
                        }
                    }
                }
                return result;
            }, cancellationToken);
        }

        protected async Task<ISystemLanguage?> CreateSystemLanguageInTA(DbTransaction ta, CultureInfo culture, bool aclWasChecked = false, CancellationToken cancellationToken = default)
        {
            if (!aclWasChecked && !await _accessService.VerifyRoleEntitlementAsync(RoleEntitlement.ModifySystemSettings, cancellationToken: cancellationToken))
            {
                throw new UnauthorizedAccessException("Not entitled to modify language");
            }
            ISystemLanguage? result = null;
            using (var cmd = ta.Connection!.CreateCommand())
            {
                var cols = new string[]
                {
                        AbstractDataAdapter.GetAdapterColumnName<SystemLanguageDataAdapter>(nameof(ISystemLanguage.IsoCode)),
                        AbstractDataAdapter.GetAdapterColumnName<SystemLanguageDataAdapter>(nameof(ISystemLanguage.Label)),
                        AbstractDataAdapter.GetAdapterColumnName<SystemLanguageDataAdapter>(nameof(ISystemLanguage.LabelNative))
                };
                var vals = new string[]
                {
                        SystemLanguageDefaults.ParamIsoCode,
                        SystemLanguageDefaults.ParamLabel,
                        SystemLanguageDefaults.ParamLabelNative
                };
                cmd.Parameters.Add(_profile.ParameterFactory.Create(vals[0], culture.IetfLanguageTag));
                cmd.Parameters.Add(_profile.ParameterFactory.Create(vals[1], culture.EnglishName));
                cmd.Parameters.Add(_profile.ParameterFactory.Create(vals[2], culture.NativeName));

                cmd.CommandText = $"{SystemLanguageConfig<TDialect>.SQLInsertLang}({string.Join(",", cols)}) VALUES (@{string.Join(",@", vals)}){EngineSpec<TDialect>.Dialect.ReturnFromInsert}";

                using (var rs = await cmd.ExecuteReaderAsync(cancellationToken))
                {
                    if (await rs.ReadAsync(cancellationToken))
                    {
                        result = new SystemLanguage(new SystemLanguageDataAdapter(rs));
                    }
                }
            }
            return result;
        }
    }
}
