using Microsoft.EntityFrameworkCore;
using QLDA.Persistence;

namespace QLDA.FakeDataTool;

/// <summary>
/// SQL Server DbContext with configurable schema (dbo/dev).
/// Injects schema via HasDefaultSchema() in OnModelCreating.
/// </summary>
public class SchemaAppDbContext(DbContextOptions<AppDbContext> options, string schema) : AppDbContext(options)
{
    private readonly string _schema = schema;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        if (!string.IsNullOrEmpty(_schema) && _schema != "dbo")
            modelBuilder.HasDefaultSchema(_schema);
    }
}