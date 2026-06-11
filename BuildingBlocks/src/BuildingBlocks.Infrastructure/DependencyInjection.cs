using BuildingBlocks.Infrastructure.DateTimes;
using BuildingBlocks.Infrastructure.Offices;
using Microsoft.Extensions.DependencyInjection;

namespace BuildingBlocks.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddDateTimeProvider(this IServiceCollection services)
    {
        services.AddSingleton<IDateTimeProvider, DateTimeProvider>();
        return services;
    }

    public static IServiceCollection AddInfrastructureLayer(this IServiceCollection services)
    {
        services.AddScoped<IExporterHelper, ExporterHelper>();
        services.AddScoped<IImporterHelper, ImporterHelper>();
        services.AddScoped<IAsposeHelper, AsposeHelper>();
        services.AddScoped<IWordHelper, WordHelper>();
        return services;
    }
}