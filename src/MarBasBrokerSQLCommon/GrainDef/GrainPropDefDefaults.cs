namespace MarBasBrokerSQLCommon.GrainDef
{
    public static class GrainPropDefDefaults
    {
        public const string DataSourcePropDef = "mb_propdef";
        public const string DataSourcePropDefExt = "mb_propdef_as_grain_with_path";

        public const string FieldValueType = "value_type";
        public const string FieldValueConstraint = "value_constraint";
        public const string FieldCardinalityMin = "cardinality_min";
        public const string FieldCardinalityMax = "cardinality_max";

        public const string ParamValueType = "valueType";
        public const string ParamCardinalityMin = "cardinalityMin";
        public const string ParamCardinalityMax = "cardinalityMax";
    }
}
