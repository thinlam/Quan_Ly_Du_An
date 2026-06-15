namespace QLDA.Application.Authorization.Markers;

/// <summary>
/// Marker interface for Commands that require authorization check (write permission).
/// Implement GetEntity() to return the entity being acted upon.
/// </summary>
public interface IAuthorizableCommand
{
    object GetEntity();
}
