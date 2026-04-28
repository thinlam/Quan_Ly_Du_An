using Microsoft.EntityFrameworkCore;
using QLDA.Persistence;

namespace QLDA.FakeDataTool;

/// <summary>
/// SQLite-compatible DbContext that clears SQL Server-specific defaults.
/// Same pattern as QLDA.Tests.TestAppDbContext and QLDA.DevSeeder.SqliteAppDbContext.
/// </summary>
public class SqliteAppDbContext(DbContextOptions<AppDbContext> options) : AppDbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        ClearSqlServerDefaults(modelBuilder);
    }

    private static void ClearSqlServerDefaults(ModelBuilder modelBuilder)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            var entityBuilder = modelBuilder.Entity(entityType.ClrType);
            foreach (var property in entityType.GetProperties().ToList())
            {
                try
                {
                    var propBuilder = entityBuilder.Property(property.Name);
                    propBuilder.HasDefaultValueSql((string?)null);
                    var columnType = property.GetColumnType();
                    if (columnType != null && columnType.Contains("max", StringComparison.OrdinalIgnoreCase))
                        propBuilder.HasColumnType("TEXT");
                }
                catch { }
            }
            foreach (var index in entityType.GetIndexes().ToList())
            {
                var propertyNames = index.Properties.Select(p => p.Name).ToArray();
                try { entityBuilder.HasIndex(propertyNames).HasFilter((string?)null); }
                catch { }
            }
        }
    }
}