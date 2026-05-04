using System.Data;
using Dapper;
using BuildingBlocks.CrossCutting.Factories;
using System.Dynamic;

namespace BuildingBlocks.Persistence.Repositories;

public class DapperRepository(IDbConnectionFactory connectionFactory) : IDapperRepository {
    public async Task<IEnumerable<T>> QueryStoredProcAsync<T>(string procName, object? param = null, string? connectionName = "DefaultConnection") {
        using var connection = connectionFactory.CreateConnection(connectionName ?? "DefaultConnection");

        try {
            return await connection.QueryAsync<T>(procName, param, commandType: CommandType.StoredProcedure);
        } catch (Exception ex) when (ex.Message.Contains("is not a parameter for procedure")) {

            // Extract param name từ error message
            var paramName = ExtractParameterNameFromError(ex.Message);

            if (!string.IsNullOrEmpty(paramName)) {
                // Tạo/update params dictionary bằng cách loại bỏ parameter không hợp lệ
                var expandoParams = ConvertToExpandoObject(param);
                var dictParams = (IDictionary<string, object?>)expandoParams;

                // Xóa param không hợp lệ
                var paramKey = paramName.TrimStart('@');
                if (dictParams.ContainsKey(paramKey)) {
                    dictParams.Remove(paramKey);
                }

                // Retry với params hợp lệ
                return await connection.QueryAsync<T>(procName, expandoParams, commandType: CommandType.StoredProcedure);
            }

            // Nếu không extract được param name, throw error gốc
            throw;
        }
    }

    public async Task<IEnumerable<T>> QueryAsync<T>(string sql, object? param = null, string? connectionName = "DefaultConnection") {
        using var connection = connectionFactory.CreateConnection(connectionName ?? "DefaultConnection");
        return await connection.QueryAsync<T>(sql, param);
    }

    /// <summary>
    /// Extract parameter name từ SQL error message
    /// VD: "@ParamName is not a parameter for procedure..." -> return "@ParamName"
    /// </summary>
    private static string ExtractParameterNameFromError(string errorMessage) {
        try {
            // Tìm vị trí của '@'
            var startIndex = errorMessage.IndexOf("@");
            if (startIndex == -1) return string.Empty;

            // Tìm ký tự space hoặc các ký tự đặc biệt sau tên parameter
            var endIndex = errorMessage.IndexOfAny(new[] { ' ', '\t', '\n', '\r' }, startIndex);
            if (endIndex == -1) {
                // Nếu không tìm thấy, lấy đến cuối chuỗi
                endIndex = errorMessage.Length;
            }

            return errorMessage.Substring(startIndex, endIndex - startIndex);
        } catch {
            return string.Empty;
        }
    }

    /// <summary>
    /// Convert object sang ExpandoObject (dynamic) để có thể add properties dynamically
    /// </summary>
    private static ExpandoObject ConvertToExpandoObject(object? param) {
        var expando = new ExpandoObject();
        var dict = (IDictionary<string, object?>)expando;

        if (param == null)
            return expando;

        // Nếu đã là dictionary
        if (param is IDictionary<string, object> paramDict) {
            foreach (var kvp in paramDict) {
                dict[kvp.Key] = kvp.Value;
            }
        } else {
            // Convert object properties sang dictionary
            var properties = param.GetType().GetProperties();
            foreach (var prop in properties) {
                dict[prop.Name] = prop.GetValue(param);
            }
        }

        return expando;
    }
}