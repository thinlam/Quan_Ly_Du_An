using BuildingBlocks.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using QLDA.Application.Authorization;
using QLDA.Application.Common.Mapping;
using QLDA.Application.DuAns.DTOs;
using QLDA.Domain.Enums;

namespace QLDA.Application.DuAns.Queries;

public record DuAnGetDanhSachExportQuery(DuAnPrintSearchDto SearchDto) : IRequest<List<DuAnExportDto>>
{
    public bool IsNoTracking { get; set; } = true;
}

internal class DuAnGetDanhSachExportQueryHandler(IServiceProvider serviceProvider)
    : IRequestHandler<DuAnGetDanhSachExportQuery, List<DuAnExportDto>>
{
    private readonly IRepository<DuAn, Guid> DuAn = serviceProvider.GetRequiredService<IRepository<DuAn, Guid>>();
    private readonly IRepository<DmDonVi, long> DmDonVi = serviceProvider.GetRequiredService<IRepository<DmDonVi, long>>();
    private readonly IRepository<UserMaster, long> UserMaster = serviceProvider.GetRequiredService<IRepository<UserMaster, long>>();
    private readonly IAuthorizationManager _authManager = serviceProvider.GetRequiredService<IAuthorizationManager>();

    public async Task<List<DuAnExportDto>> Handle(DuAnGetDanhSachExportQuery request,
        CancellationToken cancellationToken = default)
    {
        var searchDto = request.SearchDto;
        var duAnSet = request.IsNoTracking ? DuAn.GetQueryableSet().AsNoTracking() : DuAn.GetQueryableSet();
        var dmDonViSet = DmDonVi.GetQueryableSet().AsNoTracking();
        var userMasterSet = UserMaster.GetQueryableSet().AsNoTracking();

        // Reuse filter set from DuAnGetDanhSachQuery — same authorization + search semantics, no pagination.
        var queryable = _authManager.FilterVisible(duAnSet, AuthorizationResourceKeys.DuAn)
            .WhereIf(searchDto.TenDuAn.IsNotNullOrWhitespace(),
                e => e.TenDuAn!.ToLower()!.Contains(searchDto.TenDuAn!.ToLower()))
            .WhereIf(searchDto.MaDuAn.IsNotNullOrWhitespace(),
                e => e.MaDuAn!.ToLower()!.Contains(searchDto.MaDuAn!.ToLower()))
            .WhereIf(searchDto.ThoiGianKhoiCong > 0, e => e.ThoiGianKhoiCong == searchDto.ThoiGianKhoiCong)
            .WhereIf(searchDto.ThoiGianHoanThanh > 0, e => e.ThoiGianHoanThanh == searchDto.ThoiGianHoanThanh)
            .WhereIf(searchDto.MaNganSach.IsNotNullOrWhitespace(),
                e => e.MaNganSach!.ToLower().Contains(searchDto.MaNganSach!.ToLower()))
            .WhereIf(searchDto.NhomDuAnId > 0, e => e.NhomDuAnId == searchDto.NhomDuAnId)
            .WhereIf(searchDto.LoaiDuAnId > 0, e => e.LoaiDuAnId == searchDto.LoaiDuAnId)
            .WhereFunc(searchDto.DonViPhuTrachChinhId.HasValue, q => q
                .WhereIf(searchDto.DonViPhuTrachChinhId > 0, e => e.DonViPhuTrachChinhId == searchDto.DonViPhuTrachChinhId)
                .WhereIf(searchDto.DonViPhuTrachChinhId == -1, e => e.DonViPhuTrachChinhId == null)
            )
            .WhereIf(searchDto.DonViPhoiHopId > 0, e => e
                .DuAnChiuTrachNhiemXuLys!.Any(i => i.RightId == searchDto.DonViPhoiHopId && i.Loai == EChiuTrachNhiemXuLy.DonViPhoiHop)
            )
            .WhereFunc(searchDto.LanhDaoPhuTrachId.HasValue, q => q
                .WhereIf(searchDto.LanhDaoPhuTrachId > 0, e => e.LanhDaoPhuTrachId == searchDto.LanhDaoPhuTrachId)
                .WhereIf(searchDto.LanhDaoPhuTrachId == -1, e => e.LanhDaoPhuTrachId == null)
            )
            .WhereIf(searchDto.TrangThaiDuAnId > 0, e => e.TrangThaiDuAnId == searchDto.TrangThaiDuAnId)
            .WhereIf(searchDto.QuyTrinhId > 0, e => e.QuyTrinhId == searchDto.QuyTrinhId)
            .WhereFunc(searchDto.GiaiDoanId.HasValue, q => q
                .WhereIf(searchDto.GiaiDoanId > 0, e => e.GiaiDoanHienTaiId == null ? e.BuocHienTai != null && e.BuocHienTai.Buoc != null && e.BuocHienTai.Buoc.GiaiDoanId == searchDto.GiaiDoanId : e.GiaiDoanHienTaiId == searchDto.GiaiDoanId)
                .WhereIf(searchDto.GiaiDoanId == -1, e => e.GiaiDoanHienTaiId == null && e.BuocHienTaiId == null)
            )
            .WhereFunc(searchDto.LinhVucId.HasValue, q => q
                .WhereIf(searchDto.LinhVucId > 0, e => e.LinhVucId == searchDto.LinhVucId)
                .WhereIf(searchDto.LinhVucId == -1, e => e.LinhVucId == null)
            )
            .WhereFunc(searchDto.BuocId.HasValue, q => q
                .WhereIf(searchDto.BuocId > 0, e => e.BuocHienTai != null && (e.BuocHienTai.Id == searchDto.BuocId || e.BuocHienTai.BuocId == searchDto.BuocId))
                .WhereIf(searchDto.BuocId == -1, e => e.BuocHienTai == null || e.BuocHienTai.BuocId == 0)
            )
            .WhereIf(searchDto.NguonVonId > 0,
                e => e.DuAnNguonVons!.Select(i => i.RightId).Contains(searchDto.NguonVonId ?? 0))
            .WhereIf(searchDto.TuNgay.HasValue, e => e.NgayBatDau >= searchDto.TuNgay!.Value.ToStartOfDayUtc())
            .WhereIf(searchDto.DenNgay.HasValue, e => e.NgayBatDau <= searchDto.DenNgay!.Value.ToEndOfDayUtc())
            .WhereIf(searchDto.NamBatDau > 0, e => e.NgayBatDau!.Value.Year == searchDto.NamBatDau)
            .WhereIf(searchDto.NamDuAn > 0,
                e => searchDto.NamDuAn >= e.ThoiGianKhoiCong && ((e.ThoiGianHoanThanh == null && e.ThoiGianKhoiCong == searchDto.NamDuAn) || searchDto.NamDuAn <= e.ThoiGianHoanThanh))
            .WhereIf(searchDto.HinhThucDauTuId > 0, e => e.HinhThucDauTuId == searchDto.HinhThucDauTuId)
            .WhereIf(searchDto.LoaiDuAnTheoNamId > 0, e => e.LoaiDuAnTheoNamId == searchDto.LoaiDuAnTheoNamId)
            .WhereGlobalFilter(searchDto, e => e.TenDuAn);

        // Project all rows (no pagination) — same order as DuAnGetDanhSachQuery (GetQueryableSet → Index DESC).
        var rows = await queryable
            .Select(e => new
            {
                e.MaDuAn,
                e.TenDuAn,
                e.ThoiGianKhoiCong,
                e.TongMucDauTu,
                e.LanhDaoPhuTrachId,
                e.DonViPhuTrachChinhId,
                PhoiHopIds = e.DuAnChiuTrachNhiemXuLys!
                    .Where(i => i.Loai == EChiuTrachNhiemXuLy.DonViPhoiHop)
                    .Select(i => i.RightId)
                    .ToList(),
                HinhThucDauTu = e.HinhThucDauTu != null ? e.HinhThucDauTu.Ten : null,
                HinhThucQuanLy = e.HinhThucQuanLy != null ? e.HinhThucQuanLy.Ten : null,
            })
            .ToListAsync(cancellationToken);

        // Resolve display names from legacy tables (DmDonVi / UserMaster) — per CLAUDE.md these
        // are managed outside EF migrations so we use .Join() (not navigation).
        var lanhDaoIds = rows.Where(r => r.LanhDaoPhuTrachId.HasValue)
            .Select(r => r.LanhDaoPhuTrachId!.Value).Distinct().ToList();
        var donViChinhIds = rows.Where(r => r.DonViPhuTrachChinhId.HasValue)
            .Select(r => r.DonViPhuTrachChinhId!.Value).Distinct().ToList();
        var phoiHopIds = rows.SelectMany(r => r.PhoiHopIds).Distinct().ToList();

        // LanhDaoPhuTrachId on DuAn stores UserPortalId (see DuAnSearchDto XML).
        var lanhDaoDict = lanhDaoIds.Count == 0
            ? new Dictionary<long, string>()
            : await userMasterSet
                .Where(u => u.UserPortalId.HasValue && lanhDaoIds.Contains(u.UserPortalId.Value))
                .Select(u => new { PortalId = u.UserPortalId!.Value, u.HoTen })
                .ToDictionaryAsync(u => u.PortalId, u => u.HoTen ?? string.Empty, cancellationToken);

        var allDonViIds = donViChinhIds.Concat(phoiHopIds).Distinct().ToList();
        var donViDict = allDonViIds.Count == 0
            ? new Dictionary<long, string>()
            : await dmDonViSet
                .Where(dv => allDonViIds.Contains(dv.Id))
                .Select(dv => new { dv.Id, dv.TenDonVi })
                .ToDictionaryAsync(dv => dv.Id, dv => dv.TenDonVi ?? string.Empty, cancellationToken);

        return rows.Select((row, index) => new DuAnExportDto
        {
            Stt = index + 1,
            MaDuAn = row.MaDuAn,
            TenDuAn = row.TenDuAn,
            ThoiGianKhoiCong = row.ThoiGianKhoiCong,
            LanhDaoPhuTrach = row.LanhDaoPhuTrachId.HasValue
                && lanhDaoDict.TryGetValue(row.LanhDaoPhuTrachId.Value, out var lanhDao)
                ? lanhDao : null,
            DonViPhuTrachChinh = row.DonViPhuTrachChinhId.HasValue
                && donViDict.TryGetValue(row.DonViPhuTrachChinhId.Value, out var donVi)
                ? donVi : null,
            DonViPhoiHop = string.Join("; ", row.PhoiHopIds
                .Where(id => donViDict.TryGetValue(id, out _))
                .Select(id => donViDict[id])),
            HinhThucDauTu = row.HinhThucDauTu,
            HinhThucQuanLy = row.HinhThucQuanLy,
            TongMucDauTu = row.TongMucDauTu,
        }).ToList();
    }
}
