using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using BuildingBlocks.CrossCutting.Offices;
using BuildingBlocks.Infrastructure.DateTimes;
using BuildingBlocks.Infrastructure.Offices;
using BuildingBlocks.Persistence.Interceptors;
using QLDA.Infrastructure.Offices;

namespace QLDA.Infrastructure;

public static class DependencyInjection {
    public static IServiceCollection AddDateTimeProvider(this IServiceCollection services) {
        _ = services.AddSingleton<IDateTimeProvider, DateTimeProvider>();
        return services;
    }

    public static IServiceCollection AddInfrastructureDependencies(this IServiceCollection services) {
        services.AddScoped<ISaveChangesInterceptor, AuditInterceptor>();
        services.AddScoped<IExporterHelper, ExporterHelper>();
        services.AddScoped<IImporterHelper, ImporterHelper>();
        services.AddScoped<IAsposeHelper, AsposeHelper>();
        services.AddScoped<IWordHelper, WordHelper>();
        services.AddScoped<KeHoachTrienKhaiHangMucWordExporter>();
        return services;
    }
}