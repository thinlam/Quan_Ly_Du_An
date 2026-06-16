using BuildingBlocks.Domain.Entities;
using BuildingBlocks.Domain.Providers;
using Microsoft.EntityFrameworkCore;
using QLDA.Domain.Entities;

namespace QLDA.Application.DuAnBuocs.Extensions;

public static class BuocPhongBanResolver
{
    public static async Task<long?> ResolvePhongBanFromCreatorAsync(
        string createdBy,
        IRepository<UserMaster, long> userRepo,
        CancellationToken ct)
    {
        if (!long.TryParse(createdBy, out var userPortalId))
            return null;

        return await userRepo.GetQueryableSet()
            .Where(u => u.UserPortalId == userPortalId)
            .Select(u => (long?)u.PhongBanId)
            .FirstOrDefaultAsync(ct);
    }
}
