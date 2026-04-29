using QLDA.Domain.Entities;

namespace QLDA.Domain.Interfaces;

/// <summary>
/// DuAn-specific repository with custom query methods beyond generic IRepository.
/// </summary>
public interface IDuAnRepository
{
    Task<DuAn?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<DuAn>> GetAllAsync(CancellationToken ct = default);
    Task<IReadOnlyList<DuAn>> GetByTrangThaiAsync(int trangThaiDuAnId, CancellationToken ct = default);
    Task<IReadOnlyList<DuAn>> SearchAsync(string keyword, CancellationToken ct = default);
    Task AddAsync(DuAn duAn, CancellationToken ct = default);
    Task UpdateAsync(DuAn duAn, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
