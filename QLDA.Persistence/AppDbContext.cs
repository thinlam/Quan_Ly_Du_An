using System.Data;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using QLDA.Domain.Entities;

namespace QLDA.Persistence;


public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options), IUnitOfWork {

    private IDbContextTransaction _dbContextTransaction = null!;
    public bool HasTransaction => Database.CurrentTransaction != null;
    public async Task<IDisposable> BeginTransactionAsync(IsolationLevel isolationLevel = IsolationLevel.ReadCommitted,
      CancellationToken cancellationToken = default) {
        _dbContextTransaction = await Database.BeginTransactionAsync(isolationLevel, cancellationToken);
        return _dbContextTransaction;

    }

    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default) {
        await _dbContextTransaction.CommitAsync(cancellationToken);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder) {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(
            Assembly.GetExecutingAssembly(),
            t => t.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEntityTypeConfiguration<>))
        );
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) {
        if (optionsBuilder.IsConfigured)
            return;
        ManagedException.Throw("Chưa cấu hình kết nối database");
    }

    IRepository<TEntity, TKey> IUnitOfWork.GetRepository<TEntity, TKey>() {
        return new Repository<TEntity, TKey>(this);
    }

    public IJunctionRepository<TEntity> GetJunctionRepository<TEntity>() where TEntity : class {
        return new JunctionRepository<TEntity>(this);
    }

}
