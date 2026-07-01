using System.Reflection;
using BuildingBlocks.Application;
using MediatR;
using QLDA.Application.Authorization;
using QLDA.Application.Authorization.Behaviors;
using QLDA.Application.DuAns.Services;

namespace QLDA.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationDependencies(this IServiceCollection services)
    {
        services.AddApplicationLayer(Assembly.GetExecutingAssembly());

        // Authorization infrastructure
        services.AddScoped<IAuthorizationContext, AuthorizationContext>();
        services.AddScoped<IAuthorizationManager, AuthorizationManager>();
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(AuthorizationBehavior<,>));

        // BuocAuthorizationProvider for step-level authorization
        services.AddScoped<IBuocAuthorizationProvider, BuocAuthorizationProvider>();

        // DuAnAuthorizationProvider for project-level authorization
        services.AddScoped<IAuthorizationProvider, DuAnAuthorizationProvider>();

        services.AddScoped<DuAnTepDinhKemGroupIdResolver>();

        return services;
    }
}
