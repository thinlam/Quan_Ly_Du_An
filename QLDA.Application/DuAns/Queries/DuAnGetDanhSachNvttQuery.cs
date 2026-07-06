using Microsoft.EntityFrameworkCore;
using QLDA.Application.Common.Mapping;
using QLDA.Application.DuAns.DTOs;
using QLDA.Application.Providers;
using QLDA.Domain.Enums;

namespace QLDA.Application.DuAns.Queries;

/// <summary>
/// NVTT-flavored variant of <see cref="DuAnGetDanhSachQuery"/> used by the
/// dedicated <c>api/nvtt/du-an/danh-sach</c> endpoint. Role-gated at the
/// controller to NVTT_BP01 / NVTT_XemDuAn only — bypasses ownership filter so
/// NVTT sees all DuAn across the system. Behavior mirrors the standard query
/// 1:1 (same search filters, same projection, same pagination).
/// </summary>
public record DuAnGetDanhSachNvttQuery(DuAnSearchDto SearchDto) : AggregateRootPagination, IRequest<PaginatedList<DuAnDto>>
{
    public bool IsNoTracking { get; set; }
}

internal class DuAnGetDanhSachNvttQueryHandler(IServiceProvider serviceProvider) : IRequestHandler<DuAnGetDanhSachNvttQuery, PaginatedList<DuAnDto>>
{
    private readonly IRepository<DuAn, Guid> DuAn = serviceProvider.GetRequiredService<IRepository<DuAn, Guid>>();

    public async Task<PaginatedList<DuAnDto>> Handle(DuAnGetDanhSachNvttQuery request,
        CancellationToken cancellationToken = default)
    {
        var queryable = DuAn.GetQueryableSet()
            .Include(e => e.DuToans)
            .WhereIf(request.SearchDto.TenDuAn.IsNotNullOrWhitespace(),
                e => e.TenDuAn!.ToLower()!.Contains(request.SearchDto.TenDuAn!.ToLower()))
            .WhereIf(request.SearchDto.MaDuAn.IsNotNullOrWhitespace(),
                e => e.MaDuAn!.ToLower()!.Contains(request.SearchDto.MaDuAn!.ToLower()))
            .WhereIf(request.SearchDto.ThoiGianKhoiCong > 0, e => e.ThoiGianKhoiCong == request.SearchDto.ThoiGianKhoiCong)
            .WhereIf(request.SearchDto.ThoiGianHoanThanh > 0, e => e.ThoiGianHoanThanh == request.SearchDto.ThoiGianHoanThanh)
            .WhereIf(request.SearchDto.MaNganSach.IsNotNullOrWhitespace(),
                e => e.MaNganSach!.ToLower().Contains(request.SearchDto.MaNganSach!.ToLower()))
            .WhereIf(request.SearchDto.NhomDuAnId > 0, e => e.NhomDuAnId == request.SearchDto.NhomDuAnId)
            .WhereIf(request.SearchDto.LoaiDuAnId > 0, e => e.LoaiDuAnId == request.SearchDto.LoaiDuAnId)
            .WhereFunc(request.SearchDto.DonViPhuTrachChinhId.HasValue, q => q
                .WhereIf(request.SearchDto.DonViPhuTrachChinhId > 0, e => e.DonViPhuTrachChinhId == request.SearchDto.DonViPhuTrachChinhId)
                .WhereIf(request.SearchDto.DonViPhuTrachChinhId == -1, e => e.DonViPhuTrachChinhId == null)
            )
            .WhereIf(request.SearchDto.DonViPhoiHopId > 0, e => e
                .DuAnChiuTrachNhiemXuLys!.Any(i => i.RightId == request.SearchDto.DonViPhoiHopId && i.Loai == EChiuTrachNhiemXuLy.DonViPhoiHop)
            )
            .WhereFunc(request.SearchDto.LanhDaoPhuTrachId.HasValue, q => q
                .WhereIf(request.SearchDto.LanhDaoPhuTrachId > 0, e => e.LanhDaoPhuTrachId == request.SearchDto.LanhDaoPhuTrachId)
                .WhereIf(request.SearchDto.LanhDaoPhuTrachId == -1, e => e.LanhDaoPhuTrachId == null)
            )
            .WhereIf(request.SearchDto.TrangThaiDuAnId > 0, e => e.TrangThaiDuAnId == request.SearchDto.TrangThaiDuAnId)
            .WhereIf(request.SearchDto.QuyTrinhId > 0, e => e.QuyTrinhId == request.SearchDto.QuyTrinhId)
            .WhereFunc(request.SearchDto.GiaiDoanId.HasValue, q => q
                .WhereIf(request.SearchDto.GiaiDoanId > 0, e => e.GiaiDoanHienTaiId == null ? e.BuocHienTai != null && e.BuocHienTai.Buoc != null && e.BuocHienTai.Buoc.GiaiDoanId == request.SearchDto.GiaiDoanId : e.GiaiDoanHienTaiId == request.SearchDto.GiaiDoanId)
                .WhereIf(request.SearchDto.GiaiDoanId == -1, e => e.GiaiDoanHienTaiId == null && e.BuocHienTaiId == null)
            )
            .WhereFunc(request.SearchDto.LinhVucId.HasValue, q => q
                .WhereIf(request.SearchDto.LinhVucId > 0, e => e.LinhVucId == request.SearchDto.LinhVucId)
                .WhereIf(request.SearchDto.LinhVucId == -1, e => e.LinhVucId == null)
            )
            .WhereFunc(request.SearchDto.BuocId.HasValue, q => q
                .WhereIf(request.SearchDto.BuocId > 0, e => e.BuocHienTai != null && (e.BuocHienTai.Id == request.SearchDto.BuocId || e.BuocHienTai.BuocId == request.SearchDto.BuocId))
                .WhereIf(request.SearchDto.BuocId == -1, e => e.BuocHienTai == null || e.BuocHienTai.BuocId == 0)
            )
            .WhereIf(request.SearchDto.NguonVonId > 0,
                e => e.DuAnNguonVons!.Select(i => i.RightId).Contains(request.SearchDto.NguonVonId ?? 0))
            .WhereIf(request.SearchDto.TuNgay.HasValue, e => e.NgayBatDau >= request.SearchDto.TuNgay!.Value.ToStartOfDayUtc())
            .WhereIf(request.SearchDto.DenNgay.HasValue, e => e.NgayBatDau <= request.SearchDto.DenNgay!.Value.ToEndOfDayUtc())
            .WhereIf(request.SearchDto.NamBatDau > 0, e => e.NgayBatDau!.Value.Year == request.SearchDto.NamBatDau)
            .WhereIf(request.SearchDto.NamDuAn > 0,
                e => request.SearchDto.NamDuAn >= e.ThoiGianKhoiCong && ((e.ThoiGianHoanThanh == null && e.ThoiGianKhoiCong == request.SearchDto.NamDuAn) || request.SearchDto.NamDuAn <= e.ThoiGianHoanThanh))
            .WhereIf(request.SearchDto.HinhThucDauTuId > 0, e => e.HinhThucDauTuId == request.SearchDto.HinhThucDauTuId)
            .WhereIf(request.SearchDto.LoaiDuAnTheoNamId > 0, e => e.LoaiDuAnTheoNamId == request.SearchDto.LoaiDuAnTheoNamId)
            .WhereGlobalFilter(
                request.SearchDto,
                e => e.TenDuAn
            );

        return await queryable
            .Select(e => new DuAnDto()
            {
                Id = e.Id,
                TenDuAn = e.TenDuAn,
                QuyTrinhId = e.QuyTrinhId,
                DiaDiem = e.DiaDiem,
                ChuDauTuId = e.ChuDauTuId,
                ThoiGianKhoiCong = e.ThoiGianKhoiCong,
                ThoiGianHoanThanh = e.ThoiGianHoanThanh,
                MaDuAn = e.MaDuAn,
                MaNganSach = e.MaNganSach,
                DuAnTrongDiem = e.DuAnTrongDiem,
                GiaiDoanId = e.GiaiDoanHienTaiId != null ? e.GiaiDoanHienTaiId : e.BuocHienTai != null && e.BuocHienTai.Buoc != null ? e.BuocHienTai.Buoc.GiaiDoanId : null,
                LinhVucId = e.LinhVucId,
                NhomDuAnId = e.NhomDuAnId,
                NangLucThietKe = e.NangLucThietKe,
                QuyMoDuAn = e.QuyMoDuAn,
                HinhThucQuanLyDuAnId = e.HinhThucQuanLyDuAnId,
                LoaiDuAnId = e.LoaiDuAnId,
                TongMucDauTu = e.TongMucDauTu,
                TrangThaiDuAnId = e.TrangThaiDuAnId,
                GhiChu = e.GhiChu,
                NgayBatDau = e.NgayBatDau.HasValue ? e.NgayBatDau.Value.Date : null,
                LanhDaoPhuTrachId = e.LanhDaoPhuTrachId,
                DonViPhuTrachChinhId = e.DonViPhuTrachChinhId,
                DonViPhoiHopIds = e.DuAnChiuTrachNhiemXuLys!
                    .Where(i => i.Loai == EChiuTrachNhiemXuLy.DonViPhoiHop)
                    .Select(i => i.RightId).ToList(),

                #region Task #9121
                HinhThucDauTuId = e.HinhThucDauTuId,
                LoaiDuAnTheoNamId = e.LoaiDuAnTheoNamId,
                TenBuoc = e.BuocHienTai != null ? e.BuocHienTai.TenBuoc : null,
                BuocId = e.BuocHienTai != null ? e.BuocHienTai.BuocId : null,
                #endregion
            })
            .PaginatedListAsync(request.SearchDto.Skip(), request.SearchDto.Take(), cancellationToken: cancellationToken);
    }
}
