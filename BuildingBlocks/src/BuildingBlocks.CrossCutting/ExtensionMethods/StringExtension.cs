using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Reflection;

namespace BuildingBlocks.CrossCutting.ExtensionMethods;

public static class StringExtension
{

    public static bool IsNotNullOrWhitespace([NotNullWhen(true)] this string? value)
        => !string.IsNullOrWhiteSpace(value);

    /// <summary>
    /// Chuyển tên enum (string) sang DescriptionAttribute của giá trị enum tương ứng.
    /// </summary>
    /// <param name="enumName">Tên enum, ví dụ: "QuyetDinhDuyetDuAn"</param>
    /// <returns>Chuỗi description, hoặc null nếu không tìm thấy.</returns>
    public static string GetDescriptionFromName<TEnum>(this string enumName) where TEnum : struct, Enum
        => !Enum.TryParse<TEnum>(enumName, out var enumValue) ? string.Empty : enumValue.GetDescription();

    /// <summary>
    /// Lấy DescriptionAttribute từ một trường const string trong static class.
    /// Ví dụ: "PheDuyetDuToan" → "Phê duyệt dự toán"
    /// </summary>
    /// <param name="constantName">Tên trường constant, ví dụ: "PheDuyetDuToan"</param>
    /// <param name="constantsType">Type của class chứa constants, ví dụ: typeof(PheDuyetEntityNames)</param>
    /// <returns>Chuỗi description, hoặc originalValue nếu không tìm thấy.</returns>
    public static string GetDescriptionFromConstant(this string constantName, Type constantsType)
    {
        var field = constantsType.GetField(constantName, BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);
        if (field == null) return constantName;
        var desc = field.GetCustomAttribute<DescriptionAttribute>(false);
        return desc?.Description ?? constantName;
    }

    public static string GetContentType(this string filePath)
    {
        if (!File.Exists(filePath))
            return string.Empty;
        var ext = Path.GetExtension(filePath).ToLowerInvariant();
        switch (ext)
        {
            case ".xlsx":
                return "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            case ".xls":
                return "application/vnd.ms-excel";
            default:
                return "application/octet-stream";
        }
    }

    public static object? ConvertStringToPropertyType(this string cellValue, Type propertyType)
    {
        if (string.IsNullOrWhiteSpace(cellValue))
            return GetDefaultValue(propertyType);
        if (propertyType == typeof(string))
            return cellValue;

        if (propertyType == typeof(int) || propertyType == typeof(int?))
        {
            if (int.TryParse(cellValue, out var intVal))
                return intVal;
            return propertyType == typeof(int) ? 0 : null;
        }

        if (propertyType == typeof(long) || propertyType == typeof(long?))
        {
            var numberCulture = new CultureInfo("vi-VN");
            if (long.TryParse(cellValue, NumberStyles.Number, numberCulture, out var longVal)
                || long.TryParse(cellValue, NumberStyles.Number, CultureInfo.InvariantCulture, out longVal))
                return longVal;
            return propertyType == typeof(long) ? 0L : null;
        }

        if (propertyType == typeof(double) || propertyType == typeof(double?))
        {
            if (double.TryParse(cellValue, out var doubleVal))
                return doubleVal;
            return propertyType == typeof(double) ? 0.0 : null;
        }

        if (propertyType == typeof(bool) || propertyType == typeof(bool?))
        {
            if (bool.TryParse(cellValue, out var boolVal))
                return boolVal;
            return cellValue switch
            {
                // Xử lý thêm trường hợp "0" hoặc "1" nếu cần
                "0" => false,
                "1" => true,
                _ => propertyType == typeof(bool) ? false : null
            };
        }

        //ràng buộc thành format VN
        var culture = new CultureInfo("vi-VN");
        var formats = new[] { "dd/MM/yyyy", "d/M/yyyy", "dd-MM-yyyy" };
        var dateTimeStyle = DateTimeStyles.None;

        if (propertyType == typeof(DateTime) || propertyType == typeof(DateTime?))
        {
            if (DateTime.TryParseExact(cellValue, formats, culture, dateTimeStyle,
                    out var dateVal))
                return dateVal;
            return propertyType == typeof(DateTime) ? DateTime.MinValue : null;
        }

        if (propertyType == typeof(DateTimeOffset) || propertyType == typeof(DateTimeOffset?))
        {
            if (DateTimeOffset.TryParseExact(cellValue, formats, culture, dateTimeStyle,
                    out var dateVal))
                return dateVal;
            return propertyType == typeof(DateTimeOffset) ? DateTimeOffset.MinValue : null;
        }

        return GetDefaultValue(propertyType);
    }

    /// <summary>
    /// Lấy giá trị mặc định của một kiểu dữ liệu.
    /// </summary>
    private static object? GetDefaultValue(Type t)
        => t.IsValueType ? Activator.CreateInstance(t) : null;
}