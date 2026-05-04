namespace BuildingBlocks.Domain.Interfaces;

public interface IDapperRepository
{
    public Task<IEnumerable<T>> QueryStoredProcAsync<T>(string procName, object? param = null, string? connectionName = "DefaultConnection");
    public Task<IEnumerable<T>> QueryAsync<T>(string sql, object? param = null, string? connectionName = "DefaultConnection");
}
