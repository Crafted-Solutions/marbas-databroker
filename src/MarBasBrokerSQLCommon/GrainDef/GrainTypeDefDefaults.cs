namespace MarBasBrokerSQLCommon.GrainDef
{
    public static class GrainTypeDefDefaults
    {
        public const string DataSourceTypeDef = "mb_typedef";
        public const string DataSourceTypeDefExt = "mb_typedef_as_grain_with_path";
        public const string DataSourceTypeDefMixin = "mb_typedef_mixin";
        public const string DataSourceTypeDefMixinAnc = "mb_typedef_mixin_ancestor";
        public const string DataSourceTypeDefMixinDesc = "mb_typedef_mixin_descendant";

        public const string FieldDefaultInstance = "defaults_id";

        public const string MixinExtFieldStart = "start";
        public const string MixinExtFieldBaseType = "base_typedef_id";
        public const string MixinExtFieldDerivedType = "derived_typedef_id";

        public const string ParamTypeDefId = "typeDefId";
        public const string ParamTypeDefPath = "typeDefPath";
        public const string ParamImpl = "impl";
    }
}
