using System.Reflection;
using BuildingBlocks.Application;
using MediatR;
using QLDA.Application.Authorization;
using QLDA.Application.Authorization.Behaviors;

namespace QLDA.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationDependencies(this IServiceCollection services)
    {
        services.AddApplicationLayer(Assembly.GetExecutingAssembly());

        // Authorization infrastructure
        services.AddScoped<IAuthorizationContext, AuthorizationContext>();
        services.AddSingleton<IAuthorizationManager, AuthorizationManager>();
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(AuthorizationBehavior<,>));

        return services;
    }
}
