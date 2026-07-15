using BuildingBlocks.CrossCutting.DateTimes;
using Microsoft.EntityFrameworkCore;
using QLDA.Application.Authorization;
using QLDA.Application.Common.Mapping;
using QLDA.Application.DuAns.DTOs;
using QLDA.Domain.Constants;
using QLDA.Domain.Enums;

namespace QLDA.Application.DuAns.Queries;

public record TheoDoiDuAnTheoGiaiDoanQuery(TheoDoiDuAnTheoGiaiDoanSearchDto SearchDto)
    : IRequest<TheoDoiDuAnTheoGiaiDoanResultDto>;

internal class TheoDoiDuAnTheoGiaiDoanQueryHandler(
    IRepository<DuAn, Guid> duAn,
    IRepository<DanhMucTrangThaiDuAn, int> trangThaiDuAnRepo,
    IRepository<DmDonVi, long> dmDonVi,
    IRepository<UserMaster, long> userMaster,
    IAuthorizationManager authManager,
    IDateTimeProvider dateTimeProvider)
    : IRequestHandler<TheoDoiDuAnTheoGiaiDoanQuery, TheoDoiDuAnTheoGiaiDoanResultDto>
{
    /// <summary>Key cố định để EF gom mọi row vào 1 nhóm — tính nhiều Count trong 1 round-trip DB.</summary>
    private const bool AggregateAllRows = true;

    public async Task<TheoDoiDuAnTheoGiaiDoanResultDto> Handle(
        TheoDoiDuAnTheoGiaiDoanQuery request,
        CancellationToken cancellationToken = default)
    {
        var search = request.SearchDto;
        var hoanThanhId = await ResolveHoanThanhIdAsync(cancellationToken);
        var namHienTai = dateTimeProvider.OffsetUtcNow.Year;

        var root = authManager.FilterVisible(duAn.GetQueryableSet(), AuthorizationResourceKeys.DuAn);

        var counters = await BuildQuery(
                root,
                search,
                namHienTai,
                hoanThanhId,
                ETheoDoiDuAnTheoGiaiDoanLoai.TatCa)
            .GroupBy(_ => AggregateAllRows)
            .Select(g => new TheoDoiCounters(
                g.Count(e => e.TrangThaiDuAnId == hoanThanhId),
                g.Count(e => e.TrangThaiDuAnId != hoanThanhId
                    && (e.ThoiGianHoanThanh == null || namHienTai <= e.ThoiGianHoanThanh)),
                g.Count(e => e.TrangThaiDuAnId != hoanThanhId
                    && e.ThoiGianHoanThanh != null
                    && namHienTai > e.ThoiGianHoanThanh)))
            .FirstOrDefaultAsync(cancellationToken) ?? TheoDoiCounters.Empty;

        var danhSach = await ProjectToDto(
                BuildQuery(
                    root,
                    search,
                    namHienTai,
                    hoanThanhId,
                    search.Loai)
                    .OrderBy(e => e.TenDuAn))
            .PaginatedListAsync(search.Skip(), search.Take(), cancellationToken);

        ApplyStt(danhSach.Data, search.Skip());

        return new TheoDoiDuAnTheoGiaiDoanResultDto
        {
            TongSoDuAn = counters.Total,
            ConHan = counters.ConHan,
            QuaHan = counters.QuaHan,
            DaHoanThanh = counters.DaHoanThanh,
            DanhSach = danhSach,
        };
    }

    private async Task<int> ResolveHoanThanhIdAsync(CancellationToken cancellationToken)
    {
        var hoanThanh = await trangThaiDuAnRepo
            .GetQueryableSet(OnlyUsed: true, OnlyNotDeleted: true, OrderByIndex: false)
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Ma == DanhMucTrangThaiDuAnCodes.HoanThanh, cancellationToken);

        ManagedException.ThrowIfNull(hoanThanh, "Không tìm thấy trạng thái dự án 'Đã hoàn thành'");
        return hoanThanh!.Id;
    }

    private static IQueryable<DuAn> BuildQuery(
        IQueryable<DuAn> query,
        TheoDoiDuAnTheoGiaiDoanSearchDto search,
        int namHienTai,
        int hoanThanhId,
        ETheoDoiDuAnTheoGiaiDoanLoai loai)
    {
        query = query
            .AsNoTracking()
            .WhereFunc(search.GiaiDoanId.HasValue, q => q
                .WhereIf(search.GiaiDoanId > 0,
                    e => e.GiaiDoanHienTaiId == null
                        ? e.BuocHienTai != null
                          && e.BuocHienTai.Buoc != null
                          && e.BuocHienTai.Buoc.GiaiDoanId == search.GiaiDoanId
                        : e.GiaiDoanHienTaiId == search.GiaiDoanId)
                .WhereIf(search.GiaiDoanId == -1,
                    e => e.GiaiDoanHienTaiId == null && e.BuocHienTaiId == null))
            .WhereIf(search.TenDuAn.IsNotNullOrWhitespace(),
                e => e.TenDuAn!.ToLower().Contains(search.TenDuAn!.ToLower()))
            .WhereIf(search.MaDuAn.IsNotNullOrWhitespace(),
                e => e.MaDuAn!.ToLower().Contains(search.MaDuAn!.ToLower()))
            .WhereIf(search.NamDuAn > 0,
                e => search.NamDuAn >= e.ThoiGianKhoiCong
                     && ((e.ThoiGianHoanThanh == null && e.ThoiGianKhoiCong == search.NamDuAn)
                         || search.NamDuAn <= e.ThoiGianHoanThanh))
            .WhereIf(search.LoaiDuAnId > 0, e => e.LoaiDuAnId == search.LoaiDuAnId)
            .WhereFunc(search.DonViPhuTrachChinhId.HasValue, q => q
                .WhereIf(search.DonViPhuTrachChinhId > 0, e => e.DonViPhuTrachChinhId == search.DonViPhuTrachChinhId)
                .WhereIf(search.DonViPhuTrachChinhId == -1, e => e.DonViPhuTrachChinhId == null))
            .WhereIf(search.ThoiGianKhoiCong > 0, e => e.ThoiGianKhoiCong == search.ThoiGianKhoiCong)
            .WhereIf(search.ThoiGianHoanThanh > 0, e => e.ThoiGianHoanThanh == search.ThoiGianHoanThanh)
            .WhereIf(search.TrangThaiDuAnId > 0, e => e.TrangThaiDuAnId == search.TrangThaiDuAnId)
            .WhereIf(
                search.LinhVucId.HasValue && search.LinhVucId.Value > 0,
                e => e.LinhVucId == search.LinhVucId!.Value)
            .WhereGlobalFilter(search, e => e.TenDuAn, e => e.MaDuAn);

        return loai switch
        {
            ETheoDoiDuAnTheoGiaiDoanLoai.DaHoanThanh => query.Where(e => e.TrangThaiDuAnId == hoanThanhId),
            ETheoDoiDuAnTheoGiaiDoanLoai.ConHan => query.Where(e => e.TrangThaiDuAnId != hoanThanhId
                && (e.ThoiGianHoanThanh == null || namHienTai <= e.ThoiGianHoanThanh)),
            ETheoDoiDuAnTheoGiaiDoanLoai.QuaHan => query.Where(e => e.TrangThaiDuAnId != hoanThanhId
                && e.ThoiGianHoanThanh != null
                && namHienTai > e.ThoiGianHoanThanh),
            _ => query,
        };
    }

    private IQueryable<TheoDoiDuAnTheoGiaiDoanDto> ProjectToDto(IQueryable<DuAn> query)
    {
        var users = userMaster.GetQueryableSet().AsNoTracking();
        var donVis = dmDonVi.GetQueryableSet().AsNoTracking();

        return query.Select(e => new TheoDoiDuAnTheoGiaiDoanDto
        {
            Id = e.Id,
            MaDuAn = e.MaDuAn,
            TenDuAn = e.TenDuAn,
            GiaiDoanId = e.GiaiDoanHienTaiId != null
                ? e.GiaiDoanHienTaiId
                : e.BuocHienTai != null && e.BuocHienTai.Buoc != null
                    ? e.BuocHienTai.Buoc.GiaiDoanId
                    : null,
            TenGiaiDoan = e.GiaiDoanHienTai != null
                ? e.GiaiDoanHienTai.Ten
                : e.BuocHienTai != null && e.BuocHienTai.Buoc != null && e.BuocHienTai.Buoc.GiaiDoan != null
                    ? e.BuocHienTai.Buoc.GiaiDoan.Ten
                    : null,
            ThoiGianKhoiCong = e.ThoiGianKhoiCong,
            ThoiGianHoanThanh = e.ThoiGianHoanThanh,
            ThoiGianThucHien = e.ThoiGianKhoiCong == null && e.ThoiGianHoanThanh == null
                ? null
                : e.ThoiGianKhoiCong != null && e.ThoiGianHoanThanh != null
                    ? e.ThoiGianKhoiCong.ToString() + " - " + e.ThoiGianHoanThanh.ToString()
                    : (e.ThoiGianKhoiCong ?? e.ThoiGianHoanThanh)!.ToString(),
            HinhThucQuanLyDuAnId = e.HinhThucQuanLyDuAnId,
            HinhThucDauTuId = e.HinhThucDauTuId,
            HinhThucQuanLyDuAn = e.HinhThucQuanLy != null ? e.HinhThucQuanLy.Ten : null,
            HinhThucDauTu = e.HinhThucDauTu != null ? e.HinhThucDauTu.Ten : null,
            TongMucDauTu = e.TongMucDauTu,
            DonViPhuTrachChinhId = e.DonViPhuTrachChinhId,
            TrangThaiDuAnId = e.TrangThaiDuAnId,
            LanhDaoPhuTrach = e.LanhDaoPhuTrachId == null
                ? null
                : users.Where(u => u.Id == e.LanhDaoPhuTrachId).Select(u => u.HoTen).FirstOrDefault(),
            DonViPhuTrachChinh = e.DonViPhuTrachChinhId == null
                ? null
                : donVis.Where(d => d.Id == e.DonViPhuTrachChinhId).Select(d => d.TenDonVi).FirstOrDefault(),
        });
    }

    private static void ApplyStt(List<TheoDoiDuAnTheoGiaiDoanDto> items, int skip)
    {
        for (var i = 0; i < items.Count; i++)
            items[i].Stt = skip + i + 1;
    }

    private sealed record TheoDoiCounters(int DaHoanThanh, int ConHan, int QuaHan)
    {
        public static TheoDoiCounters Empty { get; } = new(0, 0, 0);
        public int Total => ConHan + QuaHan + DaHoanThanh;
    }
}
