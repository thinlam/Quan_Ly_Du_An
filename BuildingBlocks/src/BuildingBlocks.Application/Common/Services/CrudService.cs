using Microsoft.EntityFrameworkCore;

namespace BuildingBlocks.Application.Common.Services;

public class CrudService<T, TKey>(IRepository<T, TKey> repository) : ICrudService<T, TKey>
    where T : class, IHasKey<TKey>, IAggregateRoot, new()
    where TKey : notnull
{
    private readonly IUnitOfWork _unitOfWork = repository.UnitOfWork;

    public Task<List<T>> GetAsync(CancellationToken cancellationToken = default)
    {
        return repository.GetQueryableSet().ToListAsync(cancellationToken);
    }

    public Task<T> GetByIdAsync(TKey id, CancellationToken cancellationToken = default)
    {
        ManagedException.ThrowIf(EqualityComparer<TKey>.Default.Equals(id, default(TKey)), "Invalid Id");
        return repository.GetQueryableSet().FirstOrDefaultAsync(x => x.Id!.Equals(id), cancellationToken)!;
    }

    public async Task AddOrUpdateAsync(T entity, bool isEnum = false, CancellationToken cancellationToken = default)
    {
        bool isExist = isEnum
            ? repository.GetOrderedSet().Any(e => e.Id!.Equals(entity.Id))
            : repository.GetQueryableSet(OnlyUsed:false)
                .Any(e => e.Id!.Equals(entity.Id));
        if (isExist)
        {
            await UpdateAsync(entity, cancellationToken);
        }
        else
        {
            await AddAsync(entity, cancellationToken);
        }
    }

    public async Task AddAsync(T entity, CancellationToken cancellationToken = default)
    {
        await repository.AddAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(T entity, CancellationToken cancellationToken = default)
    {
        await repository.UpdateAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(T entity, CancellationToken cancellationToken = default)
    {
        repository.Delete(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
