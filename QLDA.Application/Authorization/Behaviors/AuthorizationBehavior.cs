using System.Reflection;
using MediatR;
using QLDA.Application.Authorization.Markers;

namespace QLDA.Application.Authorization.Behaviors;

/// <summary>
/// MediatR pipeline behavior that enforces [AuthorizeResource] attributes.
/// - Reads: applies IAuthorizationManager.FilterVisible() to IFilterableQuery
/// - Writes: calls IAuthorizationManager.CanExecuteAsync() and throws ForbiddenException if denied
/// </summary>
public class AuthorizationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly IAuthorizationManager _auth;

    public AuthorizationBehavior(IAuthorizationManager auth)
    {
        _auth = auth;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken ct)
    {
        var attr = request?.GetType()
            .GetCustomAttribute<AuthorizeResourceAttribute>();

        if (attr is null)
            return await next();

        var ctx = _auth.Context;

        // Apply read filter
        if (attr.RequireView && request is IFilterableQuery fq && fq.Query is not null)
        {
            var queryType = fq.Query.GetType();
            var filterMethod = typeof(IAuthorizationManager)
                .GetMethod(nameof(IAuthorizationManager.FilterVisible))!
                .MakeGenericMethod(queryType);

            fq.Query = filterMethod.Invoke(_auth, [fq.Query, attr.ResourceKey])!;
        }

        // Check write permission
        if (attr.RequireExecute && request is IAuthorizableCommand cmd)
        {
            var allowed = await _auth.CanExecuteAsync(
                attr.ResourceKey,
                cmd.GetEntity(),
                ct);

            if (!allowed)
                throw new ForbiddenException(
                    $"User lacks permission for resource '{attr.ResourceKey}'.");
        }

        return await next();
    }
}
