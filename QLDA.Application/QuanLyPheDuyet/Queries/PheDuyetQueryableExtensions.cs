using Microsoft.EntityFrameworkCore;
using QLDA.Application.Authorization;
using QLDA.Application.QuanLyPheDuyet.DTOs;
using QLDA.Application.TepDinhKems.DTOs;
using QLDA.Domain.Constants;

namespace QLDA.Application.QuanLyPheDuyet.Queries;

internal record PheDuyetDanhSachFilter(string? Type, string? TrangThai);

internal static class PheDuyetQueryableExtensions
{
    public static List<PheDuyetListItemDto> ApplyDanhSachFilters(
        PheDuyetDanhSachFilter filter,
        IRepository<PheDuyet, Guid> pheDuyetRepo,
        IRepository<DuAn, Guid> duAnRepo,
        IRepository<DuAnBuoc, int> duAnBuocRepo,
        IRepository<Attachment, Guid>? tepDinhKemRepo,
        IAuthorizationContext authContext,
        long userId,
        bool includeAttachments)
    {
        var pheDuyetQuery = pheDuyetRepo.GetQueryableSet().AsNoTracking()
            .Where(e => !e.IsDeleted)
            .WhereIf(!string.IsNullOrEmpty(filter.Type), e => e.EntityName == filter.Type)
            .WhereIf(!string.IsNullOrEmpty(filter.TrangThai), e => e.TrangThai!.Ma == filter.TrangThai);

        var duAnQuery = duAnRepo.GetQueryableSet().AsNoTracking();
        var duAnBuocQuery = duAnBuocRepo.GetQueryableSet().AsNoTracking();

        // Không load TepDinhKem trong EF Select — subquery có thể làm lệch số dòng giữa list và export.
        var items = pheDuyetQuery
            .Join(duAnQuery, e => e.DuAnId, da => da.Id, (e, da) => new { e, da })
            .GroupJoin(duAnBuocQuery, x => x.e.BuocId, b => b.Id, (x, buocGroup) => new { x.e, x.da, buocGroup })
            .SelectMany(x => x.buocGroup.DefaultIfEmpty(), (x, b) => new { x.e, x.da, b })
            .WhereIf(!authContext.HasKhtcBypass, x => x.da.LanhDaoPhuTrachId == userId)
            .Select(x => new PheDuyetListItemDto {
            Id = x.e.Id,
            Type = filter.Type ?? "",
            EntityId = x.e.EntityId.ToString(),
            EntityName = x.e.EntityName ?? "",
            DuAnId = x.e.DuAnId,
            TenDuAn = x.da != null ? x.da.TenDuAn : null,
            TenBuoc = x.da != null && x.da.BuocHienTai != null ? x.da.BuocHienTai.TenBuoc : (x.b.TenBuoc ?? ""),
            TenGiaiDoan = x.b != null && x.b.Buoc != null && x.b.Buoc.GiaiDoan != null ? x.b.Buoc.GiaiDoan.Ten : "",
            TrichYeu = x.e.NoiDung,
            TrangThaiId = x.e.TrangThaiId,
            MaTrangThai = x.e.TrangThai != null && x.e.TrangThai!.Ma != "LEG"
                ? x.e.TrangThai!.Ma
                : TrangThaiPheDuyetCodes.Default.DuThao,
            TenTrangThai = x.e.TrangThai != null && x.e.TrangThai!.Ma != "LEG"
                ? x.e.TrangThai!.Ten
                : TrangThaiPheDuyetCodes.Default.TenDuThao,
            NguoiDuyetId = x.e.TrangThai != null && x.e.TrangThai!.Ma == "ĐD" ? x.e.NguoiXuLyId : 0,
            NguoiTrinhId = x.e.NguoiTrinhId,
            NgayXuLyMoiNhat = x.e.UpdatedAt,
            DanhSachTepDinhKem = null,
        })
        .OrderByDescending(i => i.NgayXuLyMoiNhat ?? DateTimeOffset.MinValue)
        .ToList();

        if (includeAttachments && tepDinhKemRepo != null && items.Count > 0) {
            AttachTepDinhKem(items, tepDinhKemRepo);
        }

        return items;
    }

    private static void AttachTepDinhKem(
        List<PheDuyetListItemDto> items,
        IRepository<Attachment, Guid> tepDinhKemRepo)
    {
        var groupIds = items
            .Select(i => i.EntityId)
            .Where(id => !string.IsNullOrEmpty(id))
            .Distinct()
            .ToList();

        if (groupIds.Count == 0) {
            return;
        }

        var filesByGroupId = tepDinhKemRepo.GetQueryableSet()
            .AsNoTracking()
            .Where(i => groupIds.Contains(i.GroupId))
            .AsEnumerable()
            .GroupBy(i => i.GroupId)
            .ToDictionary(
                g => g.Key,
                g => g.Select(i => i.ToDto()).ToList());

        foreach (var item in items) {
            item.DanhSachTepDinhKem = item.EntityId != null
                && filesByGroupId.TryGetValue(item.EntityId, out var files)
                ? files
                : [];
        }
    }
}
