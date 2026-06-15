namespace QLDA.Application.Authorization.Markers;

/// <summary>
/// Marks a Command/Query as requiring authorization for a specific resource.
/// Place on the request class (not the handler).
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class AuthorizeResourceAttribute : Attribute
{
    public required string ResourceKey { get; init; }
    public bool RequireView { get; init; }
    public bool RequireExecute { get; init; }
}
