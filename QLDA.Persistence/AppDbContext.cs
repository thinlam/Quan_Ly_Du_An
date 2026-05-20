using System.Data;
using BuildingBlocks.Domain.Interfaces;
using BuildingBlocks.Persistence.Configurations;
using BuildingBlocks.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Configuration;
using QLDA.Domain.Entities;
using QLDA.Persistence.Repositories;

namespace QLDA.Persistence;

/// <summary>
/// Schema-aware DbContext for QLDA module.
/// </summary>
public class AppDbContext : DbContext, IUnitOfWork
{
    private IDbContextTransaction DbContextTransaction = null!;
    protected readonly string Connection;
    protected readonly string? Schema;

    /// <summary>
    /// Primary constructor requiring IConfiguration for connection string.
    /// </summary>
    public AppDbContext(IConfiguration configuration, DbContextOptions<AppDbContext> options, string? schema = null)
        : this(options, schema)
    {
        Connection = configuration.GetConnectionString("DefaultConnection") ?? throw new Exception("Connection string not found");
    }

    /// <summary>
    /// Protected constructor for SQLite test contexts where connection string is not needed.
    /// </summary>
    protected AppDbContext(DbContextOptions<AppDbContext> options, string? schema = null) : base(options)
    {
        Connection = string.Empty;
        Schema = schema;
    }

    /// <summary>
    /// Schema name for this DbContext instance.
    /// Used by HasDefaultSchema in OnModelCreating and migration history table.
    /// </summary>
    public static readonly string MigrationsHistory = "__EFMigrationsHistory";

    public bool HasTransaction => Database.CurrentTransaction != null;

    public async Task<IDisposable> BeginTransactionAsync(IsolationLevel isolationLevel = IsolationLevel.ReadCommitted,
        CancellationToken cancellationToken = default)
    {
        DbContextTransaction = await Database.BeginTransactionAsync(isolationLevel, cancellationToken);
        return DbContextTransaction;
    }

    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        await DbContextTransaction.CommitAsync(cancellationToken);
    }

    IRepository<TEntity, TKey> IUnitOfWork.GetRepository<TEntity, TKey>()
        => new Repository<TEntity, TKey>(this);

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        var assemblies = AppDomain.CurrentDomain.GetAssemblies()
            .Where(a => a.FullName!.Contains("QLDA") || a.FullName.Contains("BuildingBlocks"));

        foreach (var assembly in assemblies)
        {
            // Apply AggregateRootConfiguration<> types (for aggregate roots)
            modelBuilder.ApplyConfigurationsFromAssembly(
                assembly,
                t => t.BaseType != null &&
                     t.BaseType.IsGenericType &&
                     t.BaseType.GetGenericTypeDefinition() == typeof(AggregateRootConfiguration<>)
            );

            // Apply IEntityTypeConfiguration<> types (for junction entities, etc.)
            modelBuilder.ApplyConfigurationsFromAssembly(
                assembly,
                t => t.GetInterfaces().Any(i =>
                    i.IsGenericType &&
                    i.GetGenericTypeDefinition() == typeof(IEntityTypeConfiguration<>)) &&
                     (t.BaseType == null ||
                      !t.BaseType.IsGenericType ||
                      t.BaseType.GetGenericTypeDefinition() != typeof(AggregateRootConfiguration<>))
            );
        }

        modelBuilder.Entity<BuildingBlocks.Domain.Entities.TepDinhKem>(e =>
        {
            e.ToTable(t => t.ExcludeFromMigrations());
        });
        modelBuilder.Entity<BuildingBlocks.Domain.Entities.Attachment>(e =>
        {
            e.ToTable(t => t.ExcludeFromMigrations());
        });
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (optionsBuilder.IsConfigured)
            return;
        optionsBuilder.UseSqlServer(Connection, x => x.MigrationsHistoryTable(MigrationsHistory, Schema ?? "dbo"));
    }

    /// <summary>
    /// Get junction repository for entities without IAggregateRoot
    /// </summary>
    public IJunctionRepository<TEntity> GetJunctionRepository<TEntity>() where TEntity : class
    {
        return new JunctionRepository<TEntity>(this);
    }
}