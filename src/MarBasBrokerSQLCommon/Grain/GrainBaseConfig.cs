namespace CraftedSolutions.MarBasBrokerSQLCommon.Grain
{
    public static class GrainBaseConfig
    {
        public const string DataSource = "mb_grain_base";
        public const string DataSourceExt = "mb_grain_with_path";
        public const string DataSourcePath = "mb_grain_ancestor";

        public const string FieldParentId = "parent_id";
        public const string FieldTypeDefId = "typedef_id";

        public const string GrainExtFieldIdPath = "id_path";

        public const string PathFieldStart = "start";
        public const string PathFieldDistance = "distance";

        public const string ParamParentId = "parentId";
        public const string ParamName = "name";
        public const string ParamOwner = "owner";
        public const string ParamMTime = "mTime";

        public const string PathParamStart = "start";

        public const string SQLSelect = $"SELECT * FROM {DataSourceExt} WHERE ";
        public const string SQLInsert = $"INSERT INTO {DataSource} ";
        public const string SQLDelete = $"DELETE FROM {DataSource} WHERE ";
        public const string SQLUpdate = $"UPDATE {DataSource} SET ";

        public const string SQLSelectTypeDef = $"SELECT {FieldTypeDefId} FROM {DataSource} WHERE ";

    }
}
