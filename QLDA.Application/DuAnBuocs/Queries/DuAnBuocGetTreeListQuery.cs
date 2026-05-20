using BuildingBlocks.Domain.Interfaces;
using BuildingBlocks.CrossCutting.ExtensionMethods;
using Microsoft.EntityFrameworkCore;
using QLDA.Application.Common.Constants;
using QLDA.Application.DanhMucBuocs.DTOs;
using QLDA.Application.DuAnBuocs.DTOs;
using QLDA.Application.DuAnBuocs.Extensions;
using QLDA.Domain.Entities.ViMaster;

namespace QLDA.Application.DuAnBuocs.Queries;

public class DuAnBuocGetTreeListQuery : IRequest<List<DuAnBuocStateDto>> {
    public Guid DuAnId { get; set; }
    public bool IsNoTracking { get; set; } = true;
}

internal class DuAnBuocGetTreeListQueryHandler(IServiceProvider serviceProvider)
    : IRequestHandler<DuAnBuocGetTreeListQuery, List<DuAnBuocStateDto>> {
    private readonly IRepository<DuAnBuoc, int> DuAnBuoc =
        serviceProvider.GetRequiredService<IRepository<DuAnBuoc, int>>();
    private readonly IRepository<DanhMucManHinh, int> DanhMucManHinh =
        serviceProvider.GetRequiredService<IRepository<DanhMucManHinh, int>>();
    private readonly IRepository<DanhMucDonVi, long> DanhMucDonVi =
        serviceProvider.GetRequiredService<IRepository<DanhMucDonVi, long>>();
    private readonly IUnitOfWork UnitOfWork =
        serviceProvider.GetRequiredService<IUnitOfWork>();

    public async Task<List<DuAnBuocStateDto>> Handle(DuAnBuocGetTreeListQuery request,
        CancellationToken cancellationToken = default) {
        var donVis = DanhMucDonVi.GetQueryableSet().AsNoTracking();
        var dbContext = UnitOfWork as DbContext;

        var baseQuery = DuAnBuoc.GetQueryableSet()
            .WhereFunc(request.IsNoTracking, q => q.AsNoTracking())
            .Include(e => e.Buoc!.GiaiDoan)
            .Include(e => e.Buoc!.QuyTrinh)
            .Include(e => e.DuAn)
            .Where(o => o.DuAnId == request.DuAnId);

        // Get all DuAnBuoc entities (for ToSteps/ToTreeList)
        var allDuAnBuocs = await baseQuery.ToListAsync(cancellationToken: cancellationToken);

        // Get all PhongBanPhuTrachChinh with LeftOuterJoin to DanhMucDonVi
        var allWithDonVi = allDuAnBuocs
            .GroupJoin(donVis, e => e.PhongPhuTrachChinhId, d => (long?)d.Id, (e, donViChinh) => new { e, donViChinh = donViChinh.FirstOrDefault() })
            .ToList();

        // Get PhongBanPhoiHops with names via DbContext
        var duAnBuocIds = allDuAnBuocs.Select(x => x.Id).ToList();
        var allPhoiHops = await dbContext!.Set<DuAnBuocPhongBanPhoiHop>()
            .Where(p => duAnBuocIds.Contains(p.LeftId))
            .Join(donVis, p => p.RightId, d => d.Id, (p, d) => new { DuAnBuocId = p.LeftId, TenDonVi = d.TenDonVi })
            .ToListAsync(cancellationToken: cancellationToken);
        var phongBanPhoiHopsGrouped = allPhoiHops
            .GroupBy(x => x.DuAnBuocId)
            .ToDictionary(g => g.Key, g => g.Select(x => x.TenDonVi!).ToList());

        var orderedSteps = allDuAnBuocs.ToSteps().ToTreeList();
        var result = orderedSteps.Join(allWithDonVi, step => step.BuocId, origin => origin.e.BuocId,
                (step, origin) => new { step, origin })
            .Select(e => new DuAnBuocStateDto() {
                Id = e.origin.e.Id,
                TenDuAn = e.origin.e.DuAn?.TenDuAn ?? ErrorMessageConstants.Unknown,
                TenQuyTrinh = e.origin.e.Buoc?.QuyTrinh?.Ten ?? ErrorMessageConstants.Unknown,
                TenGiaiDoan = e.origin.e.Buoc?.GiaiDoan?.Ten ?? ErrorMessageConstants.Unknown,
                GiaiDoanId = e.origin.e.Buoc?.GiaiDoanId,
                BuocId = e.origin.e.BuocId,
                TenBuoc = e.step.Ten,
                QuyTrinhId = e.step.Id,
                ParentId = e.step.ParentId,
                Level = e.step.Level,
                PartialView = e.origin.e.PartialView,
                Path = e.step.Path,
                Stt = e.step.Stt,
                GhiChu = e.origin.e.GhiChu,
                IsKetThuc = e.origin.e.IsKetThuc,
                NgayDuKienBatDau = e.origin.e.NgayDuKienBatDau,
                NgayDuKienKetThuc = e.origin.e.NgayDuKienKetThuc,
                NgayThucTeBatDau = e.origin.e.NgayThucTeBatDau,
                NgayThucTeKetThuc = e.origin.e.NgayThucTeKetThuc,
                TrachNhiemThucHien = e.origin.e.TrachNhiemThucHien,
                TrangThaiId = e.origin.e.TrangThaiId,
                // LeftOuterJoin với DanhMucDonVi cho phòng phụ trách chính
                PhongPhuTrachChinhId = e.origin.e.PhongPhuTrachChinhId,
                PhongBanPhuTrachChinh = e.origin.donViChinh != null ? e.origin.donViChinh.TenDonVi : null,
                // Danh sách phòng ban phối hợp
                DanhSachPhongBanPhoiHops = phongBanPhoiHopsGrouped.TryGetValue(e.origin.e.Id, out var pbh) ? pbh : []
            }).ToList();

        return result;
    }
}