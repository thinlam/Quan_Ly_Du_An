using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using QLDA.Persistence;

namespace QLDA.FakeDataTool;

/// <summary>
/// SQL Server DbContext with configurable schema (dbo/dev).
/// Injects schema via HasDefaultSchema() in OnModelCreating.
/// </summary>
public class SchemaAppDbContext : AppDbContext
{
    private readonly string _schema;

    public SchemaAppDbContext(IConfiguration configuration, DbContextOptions<AppDbContext> options, string schema)
        : base(configuration, options, schema)
    {
        _schema = schema;
    }

    public SchemaAppDbContext(DbContextOptions<AppDbContext> options, string schema)
        : base(null!, options, schema)
    {
        _schema = schema;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        if (!string.IsNullOrEmpty(_schema) && _schema != "dbo")
            modelBuilder.HasDefaultSchema(_schema);
    }
}