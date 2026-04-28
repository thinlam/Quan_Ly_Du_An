using Microsoft.EntityFrameworkCore;

namespace QLDA.Persistence;

/// <summary>
/// SQLite-compatible AppDbContext that replaces SQL Server-specific defaults
/// with SQLite equivalents. Shared by Migrator, FakeDataTool, and Tests.
/// </summary>
public class SqliteAppDbContext(DbContextOptions<AppDbContext> options) : AppDbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        ClearSqlServerDefaults(modelBuilder);
    }

    protected static void ClearSqlServerDefaults(ModelBuilder modelBuilder)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            var entityBuilder = modelBuilder.Entity(entityType.ClrType);
            foreach (var property in entityType.GetProperties().ToList())
            {
                try
                {
                    var propBuilder = entityBuilder.Property(property.Name);
                    var defaultValueSql = property.GetDefaultValueSql();

                    if (!string.IsNullOrEmpty(defaultValueSql))
                    {
                        propBuilder.HasDefaultValueSql(null);
                        if (defaultValueSql.Contains("SYSDATETIMEOFFSET", StringComparison.OrdinalIgnoreCase)
                            || defaultValueSql.Contains("GETUTCDATE", StringComparison.OrdinalIgnoreCase)
                            || defaultValueSql.Contains("GETDATE", StringComparison.OrdinalIgnoreCase))
                        {
                            if (property.ClrType == typeof(DateTimeOffset)
                                || property.ClrType == typeof(DateTimeOffset?))
                                propBuilder.HasDefaultValueSql("STRFTIME('%Y-%m-%dT%H:%M:%fZ', 'NOW')");
                            else if (property.ClrType == typeof(long))
                                propBuilder.HasDefaultValueSql("CAST((JULIANDAY('NOW') - 2440587.5) * 86400 AS INTEGER)");
                        }
                        else if (defaultValueSql.Contains("DATEDIFF", StringComparison.OrdinalIgnoreCase))
                        {
                            propBuilder.HasDefaultValueSql("CAST((JULIANDAY('NOW') - 2440587.5) * 86400 AS INTEGER)");
                        }
                        // NEWSEQUENTIALID/NEWID — EF Core generates Guid.NewGuid() client-side, just clear
                    }

                    var columnType = property.GetColumnType();
                    if (columnType != null && columnType.Contains("max", StringComparison.OrdinalIgnoreCase))
                        propBuilder.HasColumnType("TEXT");
                }
                catch { }
            }
            foreach (var index in entityType.GetIndexes().ToList())
            {
                var propertyNames = index.Properties.Select(p => p.Name).ToArray();
                try { entityBuilder.HasIndex(propertyNames).HasFilter(null); }
                catch { }
            }
        }
    }
}
