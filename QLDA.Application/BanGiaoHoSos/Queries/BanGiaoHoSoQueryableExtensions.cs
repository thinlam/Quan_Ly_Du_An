using BuildingBlocks.CrossCutting.ExtensionMethods;
using BuildingBlocks.Domain.Entities;
using QLDA.Application.Authorization;
using QLDA.Application.BanGiaoHoSos.DTOs;
using QLDA.Domain.Entities;
using QLDA.Domain.Enums;

namespace QLDA.Application.BanGiaoHoSos.Queries;

internal sealed class BanGiaoHoSoDanhSachJoinedRow
{
    public BanGiaoHoSo E { get; init; } = null!;
    public UserMaster? User { get; init; }
    public DmDonVi? DonViChuTri { get; init; }
    public DmDonVi? DonViNhan { get; init; }
}

internal static class BanGiaoHoSoQueryableExtensions
{
    public static IQueryable<BanGiaoHoSoDanhSachJoinedRow> ApplyDanhSachFilters(
        this IQueryable<BanGiaoHoSo> query,
        BanGiaoHoSoSearchDto searchDto,
        IBuocAuthorizationProvider buocAuth,
        IRepository<DuAnBuoc, int> duAnBuocRepo,
        IAuthorizationContext authContext,
        IQueryable<UserMaster> users,
        IQueryable<DmDonVi> donVis)
    {
        return buocAuth.FilterVisibleChildEntities(query, duAnBuocRepo, authContext, e => e.BuocId)
            .Where(e => e.CreatedBy == authContext.UserId.ToString())
            .WhereIf(searchDto.TrangThai.HasValue, e => (int)e.TrangThai == searchDto.TrangThai!.Value)
            .WhereIf(searchDto.DuAnId.HasValue, e => e.DuAnId == searchDto.DuAnId!.Value)
            .WhereIf(searchDto.BuocId.HasValue, e => e.BuocId == searchDto.BuocId!.Value)
            .WhereGlobalFilter(searchDto, e => e.Ma, e => e.TenHoSo, e => e.GhiChu)
            .LeftOuterJoin(users, e => e.CreatedBy, u => u.Id.ToString(), (e, user) => new { e, user })
            .LeftOuterJoin(donVis, x => x.e.PhongBanChuTriId, d => (long?)d.Id, (x, donViChuTri) => new { x.e, x.user, donViChuTri })
            .LeftOuterJoin(donVis, x => x.e.PhongBanNhanId, d => (long?)d.Id, (x, donViNhan) => new { x.e, x.user, x.donViChuTri, donViNhan })
            .OrderByDescending(x => x.e.CreatedAt)
            .Select(x => new BanGiaoHoSoDanhSachJoinedRow
            {
                E = x.e,
                User = x.user,
                DonViChuTri = x.donViChuTri,
                DonViNhan = x.donViNhan,
            });
    }

    public static string GetTrangThaiText(ETrangThaiBanGiao trangThai) =>
        trangThai switch
        {
            ETrangThaiBanGiao.KhoiTao => "Khởi tạo",
            ETrangThaiBanGiao.DaBanGiao => "Đã bàn giao",
            _ => "Không xác định"
        };
}
