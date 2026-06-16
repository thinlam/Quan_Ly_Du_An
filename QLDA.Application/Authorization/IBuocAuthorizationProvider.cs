using QLDA.Domain.Entities;

namespace QLDA.Application.Authorization;

public interface IBuocAuthorizationProvider
{
    bool HasGlobalBypass(IUserProvider user);
    Task<bool> CanExecuteStepAsync(DuAnBuoc buoc, IUserProvider user, CancellationToken ct);
    IQueryable<DuAnBuoc> FilterVisibleSteps(IQueryable<DuAnBuoc> query, IUserProvider user);
    IQueryable<T> FilterVisibleChildEntities<T>(
        IQueryable<T> query,
        IRepository<DuAnBuoc, int> buocRepo,
        IUserProvider user,
        Func<T, int?> buocIdSelector) where T : class;
}
