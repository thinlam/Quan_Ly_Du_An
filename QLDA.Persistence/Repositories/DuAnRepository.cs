using Microsoft.EntityFrameworkCore;
using QLDA.Domain.Entities;
using QLDA.Domain.Interfaces;

namespace QLDA.Persistence.Repositories;

/// <summary>
/// DuAn-specific repository wrapping generic IRepository with custom query methods.
/// Works with any EF Core provider (SQL Server, SQLite).
/// </summary>
public class DuAnRepository : IDuAnRepository
{
    private readonly IRepository<DuAn, Guid> _repo;

    public DuAnRepository(IRepository<DuAn, Guid> repo)
    {
        _repo = repo;
    }

    public async Task<DuAn?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _repo.GetQueryableSet()
            .Include(d => d.TrangThaiDuAn)
            .Include(d => d.LoaiDuAn)
            .FirstOrDefaultAsync(d => d.Id == id, ct);
    }

    public async Task<IReadOnlyList<DuAn>> GetAllAsync(CancellationToken ct = default)
    {
        return await _repo.GetQueryableSet()
            .Include(d => d.TrangThaiDuAn)
            .Include(d => d.LoaiDuAn)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<DuAn>> GetByTrangThaiAsync(int trangThaiDuAnId, CancellationToken ct = default)
    {
        return await _repo.GetQueryableSet()
            .Where(d => d.TrangThaiDuAnId == trangThaiDuAnId)
            .Include(d => d.TrangThaiDuAn)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<DuAn>> SearchAsync(string keyword, CancellationToken ct = default)
    {
        var term = keyword.ToLower();
        return await _repo.GetQueryableSet()
            .Where(d => d.TenDuAn!.ToLower().Contains(term)
                        || d.MaDuAn!.ToLower().Contains(term))
            .Include(d => d.TrangThaiDuAn)
            .Take(50)
            .ToListAsync(ct);
    }

    public async Task AddAsync(DuAn duAn, CancellationToken ct = default)
    {
        await _repo.AddAsync(duAn, ct);
    }

    public async Task UpdateAsync(DuAn duAn, CancellationToken ct = default)
    {
        await _repo.UpdateAsync(duAn, ct);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await _repo.GetQueryableSet()
            .FirstOrDefaultAsync(d => d.Id == id, ct);
        if (entity != null)
        {
            entity.IsDeleted = true;
            await _repo.UpdateAsync(entity, ct);
        }
    }

    public async Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        return await _repo.UnitOfWork.SaveChangesAsync(ct);
    }
}
