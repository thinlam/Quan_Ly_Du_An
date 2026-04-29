using BuildingBlocks.Application.Middlewares;
using Microsoft.OpenApi.Models;
using QLDA.Application.Providers;
using QLDA.WebApi.ConfigurationOptions;
using Serilog;
using Swashbuckle.AspNetCore.SwaggerUI;
using System.Reflection;
using System.Text.Json.Serialization;

namespace QLDA.WebApi;

/// <summary>
/// Extensions for configuring the host.
/// </summary>
public static class WebApiHostExtensions {
    /// <summary>
    /// Configures the host with Serilog logging.
    /// </summary>
    public static IHostBuilder ConfigureWebApiHost(this IHostBuilder host, IConfiguration configuration) {
        host.UseSerilog((context, loggerConfiguration) => {
            loggerConfiguration.ReadFrom.Configuration(configuration);
        });
        return host;
    }
}

/// <summary>
/// Extensions for adding services to the service collection.
/// </summary>
public static class WebApiServiceExtensions {
    /// <summary>
    /// Adds controllers with JSON options and cache profiles.
    /// </summary>
    public static IServiceCollection AddControllersWithJson(this IServiceCollection services) {
        services.AddControllers(options => {
            var duration = TimeSpan.FromHours(12);
            // Cache profile for combobox endpoints (12 hours client cache)
            options.CacheProfiles.Add("Combobox", new Microsoft.AspNetCore.Mvc.CacheProfile {
                Duration = (int)duration.TotalSeconds, // 12 hours
                Location = Microsoft.AspNetCore.Mvc.ResponseCacheLocation.Client,
                VaryByQueryKeys = ["*"] // Vary by all query parameters
            });
        })
            .AddJsonOptions(options => {
                options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
            });

        services.AddEndpointsApiExplorer();

        return services;
    }

    /// <summary>
    /// Adds Swagger services.
    /// </summary>
    public static IServiceCollection AddSwagger(this IServiceCollection services) {
        services.AddSwaggerGen(options => {
            options.SupportNonNullableReferenceTypes();
            options.SwaggerDoc("v1", new OpenApiInfo {
                Version = "v1",
                Title = "QLDA API",
                Description = "API quản lý dự án"
            });
            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme {
                In = ParameterLocation.Header,
                Description = "JWT Authorization header using the Bearer scheme. Example: 'Bearer {token}'",
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT"
            });
            options.AddSecurityRequirement(new OpenApiSecurityRequirement {
                {
                    new OpenApiSecurityScheme {
                        Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
                    },
                    Array.Empty<string>()
                }
            });
            var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            options.IncludeXmlComments(xmlPath);
        });

        return services;
    }

    /// <summary>
    /// Adds authentication services.
    /// </summary>
    public static IServiceCollection AddAuthentication(this IServiceCollection services, AppSettings appSettings, IWebHostEnvironment environment) {
        services.AddJwtAuthentication(appSettings.Jwt, environment);
        return services;
    }

    /// <summary>
    /// Adds common services like health checks, caching, etc.
    /// </summary>
    public static IServiceCollection AddCommonServices(this IServiceCollection services) {
        services.AddDateTimeProvider();
        services.AddHealthChecks();
        services.AddMemoryCache();
        services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
        return services;
    }

    /// <summary>
    /// Adds project-specific dependencies.
    /// </summary>
    public static IServiceCollection AddProjectDependencies(this IServiceCollection services, IConfiguration configuration, AppSettings appSettings) {
        services.Configure<AppSettings>(configuration);
        services.AddSingleton<IAppSettingsProvider, AppSettingsProvider>();
        services.AddScoped<IPolicyProvider, PolicyProvider>();

        services
            .AddApplicationDependencies()
            .AddInfrastructureDependencies()
            .AddPersistence(appSettings.ConnectionStrings);

        return services;
    }

    /// <summary>
    /// Adds all web API services to the service collection.
    /// </summary>
    public static IServiceCollection AddWebApiServices(this IServiceCollection services, IConfiguration configuration, AppSettings appSettings, IWebHostEnvironment environment) {
        services.AddControllersWithJson();
        services.AddSwagger();
        services.AddAuthentication(appSettings, environment);
        services.AddCommonServices();
        services.AddProjectDependencies(configuration, appSettings);
        services.AddResponseCaching(); // Required for VaryByQueryKeys

        return services;
    }
}

/// <summary>
/// Extensions for configuring the web application.
/// </summary>
public static class WebApiAppExtensions {
    /// <summary>
    /// Central place to add web API specific middlewares.
    /// Currently ensures authentication/authorization middleware are added.
    /// </summary>
    public static WebApplication UseWebApiAppBuilder(this WebApplication app) {
        app.UseAuthentication();
        app.UseAuthorization();
        return app;
    }

    /// <summary>
    /// Adds Swagger and SwaggerUI middleware.
    /// </summary>
    public static WebApplication UseSwaggerWithUI(this WebApplication app, AppSettings appSettings) {

        // Giữ nguyên logic cho môi trường Development
        if (app.Environment.IsDevelopment()) {
            app.UseSwagger();
            app.UseSwaggerUI(c => {
                c.EnableFilter();
                c.DocExpansion(DocExpansion.None);
            });
            return app;
        }

        // Cấu hình cho môi trường Staging
        if (app.Environment.IsStaging()) {
            var pathBase = appSettings.SwaggerPathBase;
            if (string.IsNullOrEmpty(pathBase)) {
                return app;
            }

            app.UseSwagger(c => {
                c.PreSerializeFilters.Add((swaggerDoc, httpReq) => {

                    string prefix = pathBase.TrimEnd('/');

                    // TẠO URL GỐC CHÍNH XÁC: scheme + host + prefix (vd: http://192.168.1.75/qlda)
                    string baseUrl = $"{httpReq.Scheme}://{httpReq.Host.Value}{prefix}";

                    swaggerDoc.Servers =
                    [
                        // Đặt URL gốc chính xác vào tài liệu Swagger (OpenAPI Specification)
                        new OpenApiServer { Url = baseUrl }
                    ];
                });
            });

            app.UseSwaggerUI(c => {
                c.EnableFilter();
                c.DocExpansion(DocExpansion.None);
                c.SwaggerEndpoint($"{pathBase}/swagger/v1/swagger.json", "QLDA API v1");
                c.RoutePrefix = "swagger";
            });
        }

        return app;
    }
    /// <summary>
    /// Adds CORS middleware.
    /// </summary>
    public static WebApplication UseCorsPolicy(this WebApplication app) {
        app.UseCors(options => {
            options.AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader();
        });
        return app;
    }

    /// <summary>
    /// Adds logging middleware.
    /// </summary>
    public static WebApplication UseLogging(this WebApplication app) {
        app.UseSerilogRequestLogging();
        return app;
    }

    /// <summary>
    /// Adds health checks endpoint.
    /// </summary>
    public static WebApplication UseHealthChecksEndpoint(this WebApplication app) {
        app.UseHealthChecks("/health");
        return app;
    }

    /// <summary>
    /// Maps controllers.
    /// </summary>
    public static WebApplication UseControllers(this WebApplication app) {
        app.MapControllers();
        return app;
    }

    /// <summary>
    /// Configures the web application with common settings.
    /// </summary>
    public static WebApplication UseWebApiConfiguration(this WebApplication app, AppSettings appSettings) {
        app.UseMiddleware<ExceptionMiddleware>();
        app.UseSwaggerWithUI(appSettings);
        app.UseCorsPolicy();
        app.UseResponseCaching(); // Required for VaryByQueryKeys
        app.UseLogging();
        app.UseWebApiAppBuilder();
        app.UseHealthChecksEndpoint();
        app.UseControllers();

        return app;
    }
}
