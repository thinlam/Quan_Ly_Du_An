namespace QLDA.Application.Authorization.Markers;

/// <summary>
/// Marker interface for Queries that need authorization filter applied.
/// The Query property is set by the pipeline before handler executes.
/// </summary>
public interface IFilterableQuery
{
    object? Query { get; set; }
}
