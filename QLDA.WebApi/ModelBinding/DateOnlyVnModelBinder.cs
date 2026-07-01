using BuildingBlocks.CrossCutting.ExtensionMethods;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace QLDA.WebApi.ModelBinding;

public sealed class DateOnlyVnModelBinder : IModelBinder
{
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

        if (DateOnlyExtensions.TryParseFromQuery(value, out var date))
        {
            bindingContext.Result = ModelBindingResult.Success(date);
            return Task.CompletedTask;
        }

        bindingContext.ModelState.TryAddModelError(
            bindingContext.ModelName,
            $"Giá trị '{value}' không hợp lệ. Định dạng: dd-MM-yyyy.");
        bindingContext.Result = ModelBindingResult.Failed();
        return Task.CompletedTask;
    }
}

public sealed class DateOnlyVnModelBinderProvider : IModelBinderProvider
{
    public IModelBinder? GetBinder(ModelBinderProviderContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        return context.Metadata.ModelType is Type t && (t == typeof(DateOnly) || t == typeof(DateOnly?))
            ? new DateOnlyVnModelBinder()
            : null;
    }
}
