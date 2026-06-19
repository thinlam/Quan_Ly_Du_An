namespace BuildingBlocks.CrossCutting.Exceptions;

/// <summary>
/// Thrown when the current user lacks permission for the requested action.
/// Surfaced by ExceptionMiddleware as HTTP 403 with the standard ResultApi body.
/// </summary>
public class ForbiddenException : ManagedException
{
    public ForbiddenException(string message) : base(message) { }
}
