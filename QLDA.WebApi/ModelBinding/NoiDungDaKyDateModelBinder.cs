using System.Globalization;
using BuildingBlocks.CrossCutting.ExtensionMethods;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace QLDA.WebApi.ModelBinding;

/// <summary>
/// Bind ngày cho tab Nội dung đã ký: dd-MM-yyyy hoặc dd-MM (ngày-tháng, không năm).
/// dd-MM trên tuNgay → cùng ngày-tháng năm trước; dd-MM trên denNgay → hôm nay.
/// </summary>
public sealed class NoiDungDaKyDateModelBinder : IModelBinder
{
    private static readonly string[] DayMonthFormats = ["dd-MM", "dd/MM"];

    public Task BindModelAsync(ModelBindingContext bindingContext)
    {
        ArgumentNullException.ThrowIfNull(bindingContext);

        var value = bindingContext.ValueProvider.GetValue(bindingContext.ModelName).FirstValue;
        var isNullable = bindingContext.ModelType == typeof(DateOnly?);

        if (string.IsNullOrWhiteSpace(value))
        {
            bindingContext.Result = isNullable
                ? ModelBindingResult.Success(null)
                : ModelBindingResult.Failed();
            return Task.CompletedTask;
        }

        if (DateOnlyExtensions.TryParseFromQuery(value, out var fullDate))
        {
            bindingContext.Result = ModelBindingResult.Success(fullDate);
            return Task.CompletedTask;
        }

        if (TryParseDayMonth(value, out var day, out var month))
        {
            var today = DateOnly.FromDateTime(DateTime.UtcNow.Add(DateOnlyExtensions.VnOffset));
            var isDenNgay = bindingContext.ModelName.Contains("DenNgay", StringComparison.OrdinalIgnoreCase);
            var date = isDenNgay
                ? today
                : new DateOnly(today.Year - 1, month, day);

            bindingContext.Result = ModelBindingResult.Success(date);
            return Task.CompletedTask;
        }

        bindingContext.ModelState.TryAddModelError(
            bindingContext.ModelName,
            $"Giá trị '{value}' không hợp lệ. Định dạng: dd-MM-yyyy hoặc dd-MM.");
        bindingContext.Result = ModelBindingResult.Failed();
        return Task.CompletedTask;
    }

    private static bool TryParseDayMonth(string value, out int day, out int month)
    {
        day = 0;
        month = 0;
        var trimmed = value.Trim();

        if (!DateOnly.TryParseExact(trimmed, DayMonthFormats, CultureInfo.InvariantCulture,
                DateTimeStyles.None, out var parsed))
            return false;

        day = parsed.Day;
        month = parsed.Month;
        return true;
    }
}

public sealed class NoiDungDaKyDateModelBinderProvider : IModelBinderProvider
{
    public IModelBinder? GetBinder(ModelBinderProviderContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (context.Metadata.ContainerType != typeof(QLDA.Application.KySos.DTOs.NoiDungDaKySearchDto))
            return null;

        if (context.Metadata.PropertyName is not ("TuNgay" or "DenNgay"))
            return null;

        return context.Metadata.ModelType is Type t && (t == typeof(DateOnly) || t == typeof(DateOnly?))
            ? new NoiDungDaKyDateModelBinder()
            : null;
    }
}
