using System.Net;
using System.Net.Mime;
using System.Text.RegularExpressions;
using BuildingBlocks.Domain.Constants;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.SqlClient;

namespace BuildingBlocks.Application.Middlewares;

public class ExceptionMiddleware(RequestDelegate next) {
    private readonly Serilog.ILogger _logger = Serilog.Log.ForContext<ExceptionMiddleware>();

    public async Task InvokeAsync(HttpContext context) {
        try {
            await next(context);
        } catch (Exception ex) when (TryUnwrapForbidden(ex, out var forbidden)) {
            // Authorization failures return HTTP 200 but with StatusCode=403 in body
            // so callers can branch on body status code while keeping HTTP channel "OK".
            await HandleException(context, HttpStatusCode.OK, forbidden, forbidden.Message, bodyStatusCode: HttpStatusCode.Forbidden);
        } catch (ManagedException ex) {
            await HandleException(context, HttpStatusCode.OK, ex, ex.Message);
        } catch (Exception ex) {
            if (ex.InnerException is SqlException sqlEx) {
                var msg = sqlEx.Number switch {
                    245 => ParseDataTypeConversionError(sqlEx),
                    251 => ParseDataTypeConversionError(sqlEx),
                    515 => ParseNotNullError(sqlEx),
                    547 => ParseForeignKeyError(sqlEx),
                    2601 => ParseDuplicateKeyError(sqlEx),
                    2627 => ParseUniqueConstraintError(sqlEx),
                    _ => null
                };

                if (msg != null) {
                    await HandleException(context, HttpStatusCode.OK, ex, msg);
                    return;
                }
            }
            await HandleException(context, HttpStatusCode.BadRequest, ex, null);
        }
    }

    /// <summary>
    /// Recognizes ForbiddenException (or a wrapper whose innermost exception is one)
    /// so that authorization failures return 403 regardless of where they surface in
    /// the call stack.
    /// </summary>
    private static bool TryUnwrapForbidden(Exception ex, out ForbiddenException forbidden) {
        for (var current = ex; current is not null; current = current.InnerException) {
            if (current is ForbiddenException f) {
                forbidden = f;
                return true;
            }
        }

        forbidden = null!;
        return false;
    }

    /// <summary>
    /// Parses SqlException 547 to extract detailed FK error message.
    /// FK constraint naming: FK_{DependentTable}_{PrincipalTable}_{FKColumn}
    /// </summary>
    private static string ParseForeignKeyError(SqlException sqlEx) {
        var message = sqlEx.Message;

        // Extract constraint name via regex
        var constraintMatch = Regex.Match(message, @"constraint ""(\w+)""");
        if (!constraintMatch.Success) return "Khoá ngoại không hợp lệ";

        var constraintName = constraintMatch.Groups[1].Value;
        var parts = constraintName.Split('_');

        // FK_{DependentTable}_{PrincipalTable}_{FKColumn}
        if (parts.Length >= 4 && parts[0] == "FK") {
            var fkColumn = parts[3];        // e.g., GiamDocId
            var principalTable = parts[2];  // e.g., DanhMucGiamDoc

            return $"{fkColumn} -> Khoá ngoại liên kết bảng {principalTable} không tồn tại";
        }

        return "Khoá ngoại không hợp lệ";
    }

    /// <summary>
    /// Parses error 245/251 - data type conversion failure.
    /// Message: "Conversion failed when converting the [source] value '[val]' to data type [target]."
    /// </summary>
    private static string ParseDataTypeConversionError(SqlException sqlEx) {
        var message = sqlEx.Message;

        // "Conversion failed when converting the varchar value 'abc' to data type int."
        var match = Regex.Match(message,
            @"converting the (\w+)\s+value\s+'[^']*'\s+to\s+data\s+type\s+(\w+)",
            RegexOptions.IgnoreCase);

        if (match.Success) {
            var sourceType = match.Groups[1].Value;
            var targetType = match.Groups[2].Value;
            return $"Sai kiểu dữ liệu: không thể chuyển từ {sourceType} sang {targetType}";
        }

        return "Sai kiểu dữ liệu";
    }

    /// <summary>
    /// Parses error 515 - NULL violation.
    /// Message: "Cannot insert the value NULL into column '[col]', table '[table]'; column does not allow nulls."
    /// </summary>
    private static string ParseNotNullError(SqlException sqlEx) {
        var message = sqlEx.Message;

        var colMatch = Regex.Match(message, @"column\s+'([^']+)'", RegexOptions.IgnoreCase);
        var tableMatch = Regex.Match(message, @"table\s+'([^']+)'", RegexOptions.IgnoreCase);

        if (colMatch.Success && tableMatch.Success) {
            var column = colMatch.Groups[1].Value;
            var table = ExtractTableName(tableMatch.Groups[1].Value);
            return $"Giá trị không được để trống: cột [{column}] trong bảng [{table}]";
        }

        if (colMatch.Success) {
            return $"Giá trị không được để trống: cột [{colMatch.Groups[1].Value}]";
        }

        return "Giá trị không được để trống";
    }

    /// <summary>
    /// Parses error 2601 - duplicate key with unique index.
    /// Message: "Cannot insert duplicate key row in object '[schema].[table]' with unique index '[index]'. The duplicate key value is [value]."
    /// </summary>
    private static string ParseDuplicateKeyError(SqlException sqlEx) {
        var message = sqlEx.Message;

        var tableMatch = Regex.Match(message, @"object\s+'([^']+)'", RegexOptions.IgnoreCase);
        var indexMatch = Regex.Match(message, @"index\s+'([^']+)'", RegexOptions.IgnoreCase);
        var valueMatch = Regex.Match(message, @"duplicate key value is\s+\(?([^).]+)\)?", RegexOptions.IgnoreCase);

        var table = tableMatch.Success ? ExtractTableName(tableMatch.Groups[1].Value) : null;
        var index = indexMatch.Success ? indexMatch.Groups[1].Value : null;
        var value = valueMatch.Success ? valueMatch.Groups[1].Value.Trim() : null;

        if (table != null && index != null)
            return $"Khoá chính đã tồn tại: bảng [{table}], chỉ mục [{index}]{(value != null ? $", giá trị trùng: {value}" : "")}";

        if (table != null)
            return $"Khoá chính đã tồn tại: bảng [{table}]{(value != null ? $", giá trị trùng: {value}" : "")}";

        return "Khoá chính đã tồn tại";
    }

    /// <summary>
    /// Parses error 2627 - unique constraint or PK violation.
    /// Message: "Violation of UNIQUE KEY constraint '[constraint]'. Cannot insert duplicate key in object '[table]'. The duplicate key value is (value)."
    /// or: "Violation of PRIMARY KEY constraint '[PK]'. Cannot insert duplicate key in object '[table]'."
    /// </summary>
    private static string ParseUniqueConstraintError(SqlException sqlEx) {
        var message = sqlEx.Message;

        var constraintMatch = Regex.Match(message, @"constraint\s+'([^']+)'", RegexOptions.IgnoreCase);
        var tableMatch = Regex.Match(message, @"object\s+'([^']+)'", RegexOptions.IgnoreCase);
        var valueMatch = Regex.Match(message, @"duplicate key value is\s+\(?([^).]+)\)?", RegexOptions.IgnoreCase);

        var constraint = constraintMatch.Success ? constraintMatch.Groups[1].Value : null;
        var table = tableMatch.Success ? ExtractTableName(tableMatch.Groups[1].Value) : null;
        var value = valueMatch.Success ? valueMatch.Groups[1].Value.Trim() : null;

        var isPK = message.Contains("PRIMARY KEY", StringComparison.OrdinalIgnoreCase);

        if (constraint != null && table != null) {
            var type = isPK ? "khóa chính" : "ràng buộc duy nhất";
            return $"Trường dữ liệu trùng lặp: {type} [{constraint}] trong bảng [{table}]{(value != null ? $", giá trị trùng: {value}" : "")}";
        }

        if (table != null)
            return $"Trường dữ liệu trùng lặp trong bảng [{table}]{(value != null ? $", giá trị trùng: {value}" : "")}";

        return "Trường dữ liệu trùng lặp";
    }

    /// <summary>
    /// Extracts clean table name from "[schema].[table]" or "[dbo].[table]" format.
    /// </summary>
    private static string ExtractTableName(string rawTable) {
        var parts = rawTable.Split('.');
        return parts.Length > 1 ? parts[^1].Trim('[', ']') : rawTable.Trim('[', ']');
    }

    private async Task HandleException(HttpContext context, HttpStatusCode httpStatusCode, Exception exception, string? customMessage = null, HttpStatusCode? bodyStatusCode = null) {
        context.Response.ContentType = MediaTypeNames.Application.Json;
        context.Response.StatusCode = (int)httpStatusCode;
        var statusCodeForBody = (int)(bodyStatusCode ?? httpStatusCode);

        if (exception is ManagedException { Errors: not null } managedEx) {
            var err = ResultApi.Fail(string.Join(",", managedEx.Errors.SelectMany(e => e.Value)));
            err.StatusCode = statusCodeForBody;
            _logger.Error(string.Join(",", managedEx.Errors.SelectMany(e => e.Value)));
            await context.Response.WriteAsJsonAsync(err);
        } else {
            var errorMessage = customMessage ?? ErrorMessageConstants.InternalServerError;
            var err = ResultApi.Fail(errorMessage);
            err.StatusCode = statusCodeForBody;
            _logger.Error(exception, "An error occurred with custom message: {CustomMessage}. Full details: {ExceptionMessage}",
                errorMessage, exception.Message);
            await context.Response.WriteAsJsonAsync(err);
        }
    }
}
