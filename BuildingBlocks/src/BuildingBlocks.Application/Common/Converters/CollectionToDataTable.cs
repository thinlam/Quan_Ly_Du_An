using System.Reflection;
namespace BuildingBlocks.Application.Common.Converters {

    public static class DataTableConvertExtensions {
        public static DataTable ToDataTable<T>(this ICollection<T> data) {
            var table = new DataTable(typeof(T).Name);

            var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            // Create columns
            foreach (var prop in properties) {
                var type = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;
                table.Columns.Add(prop.Name, type);
            }

            // Fill rows
            foreach (var item in data) {
                var values = properties
                    .Select(p => p.GetValue(item) ?? DBNull.Value)
                    .ToArray();

                table.Rows.Add(values);
            }

            return table;
        }
        public static DataTable CreateDataTable<T>(string? tableName = null) {
            var table = new DataTable(tableName ?? typeof(T).Name);

            var properties = typeof(T).GetProperties();

            foreach (var prop in properties) {
                var type = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;
                table.Columns.Add(prop.Name, type);
            }

            return table;
        }
    }
}
