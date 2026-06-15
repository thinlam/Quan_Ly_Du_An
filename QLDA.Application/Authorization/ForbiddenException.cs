namespace QLDA.Application.Authorization;

/// <summary>
/// Thrown when user lacks permission for an authorization action.
/// </summary>
public class ForbiddenException : ManagedException
{
    public ForbiddenException(string message) : base(message) { }
}
