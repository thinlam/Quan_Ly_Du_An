using Microsoft.EntityFrameworkCore;
using QLDA.Application.Common.Mapping;
using QLDA.Application.DuAns.DTOs;
using QLDA.Domain.Entities;

namespace QLDA.Application.DuAns.Queries;

public record BaoCaoDuAnGetDanhSachQuery(BaoCaoDuAnSearchDto SearchDto)
    : AggregateRootPagination, IRequest<PaginatedList<BaoCaoDuAnDto>> {
    public bool IsNoTracking { get; set; } = true;
}

internal class BaoCaoDuAnGetDanhSachQueryHandler
    : IRequestHandler<BaoCaoDuAnGetDanhSachQuery, PaginatedList<BaoCaoDuAnDto>> {

    private readonly IRepository<DuAn, Guid> _duAn;
    private readonly IRepository<NghiemThu, Guid> _nghiemThu;
    private readonly IRepository<ThanhToan, Guid> _thanhToan;

    public BaoCaoDuAnGetDanhSachQueryHandler(IServiceProvider serviceProvider) {
        _duAn = serviceProvider.GetRequiredService<IRepository<DuAn, Guid>>();
        _nghiemThu = serviceProvider.GetRequiredService<IRepository<NghiemThu, Guid>>();
        _thanhToan = serviceProvider.GetRequiredService<IRepository<ThanhToan, Guid>>();
    }

    public async Task<PaginatedList<BaoCaoDuAnDto>> Handle(
        BaoCaoDuAnGetDanhSachQuery request,
        CancellationToken cancellationToken = default) {

        var queryable = _duAn.GetQueryableSet()
            .AsNoTracking()
            .Where(e => !e.IsDeleted)
            .Include(e => e.BuocHienTai)
            .Include(e => e.GiaiDoanHienTai)
            .WhereIf(request.SearchDto.TenDuAn.IsNotNullOrWhitespace(),
                e => e.TenDuAn!.ToLower().Contains(request.SearchDto.TenDuAn!.ToLower()))
            .WhereIf(request.SearchDto.ThoiGianKhoiCong > 0,
                e => e.ThoiGianKhoiCong == request.SearchDto.ThoiGianKhoiCong)
            .WhereIf(request.SearchDto.ThoiGianHoanThanh > 0,
                e => e.ThoiGianHoanThanh == request.SearchDto.ThoiGianHoanThanh)
            .WhereIf(request.SearchDto.LoaiDuAnTheoNamId > 0,
                e => e.LoaiDuAnTheoNamId == request.SearchDto.LoaiDuAnTheoNamId)
            .WhereIf(request.SearchDto.HinhThucDauTuId > 0,
                e => e.HinhThucDauTuId == request.SearchDto.HinhThucDauTuId)
            .WhereIf(request.SearchDto.LoaiDuAnId > 0,
                e => e.LoaiDuAnId == request.SearchDto.LoaiDuAnId)
            .WhereFunc(request.SearchDto.DonViPhuTrachChinhId.HasValue, q => q
                .WhereIf(request.SearchDto.DonViPhuTrachChinhId > 0, e => e.DonViPhuTrachChinhId == request.SearchDto.DonViPhuTrachChinhId)
                .WhereIf(request.SearchDto.DonViPhuTrachChinhId == -1, e => e.DonViPhuTrachChinhId == null)
            )
            .WhereGlobalFilter(request.SearchDto, e => e.TenDuAn);

        // Paginate DuAn first
        var totalCount = await queryable.CountAsync(cancellationToken);
        var duAnList = await queryable
            .Skip(request.SearchDto.Skip())
            .Take(request.SearchDto.Take())
            .ToListAsync(cancellationToken);

        // Aggregate NghiemThu and ThanhToan for paginated DuAn IDs
        var duAnIds = duAnList.Select(e => e.Id).ToList();

        var nghiemThuDict = duAnIds.Count == 0 ? new Dictionary<Guid, long>()
            : await _nghiemThu.GetQueryableSet()
                .AsNoTracking()
                .Where(e => !e.IsDeleted && duAnIds.Contains(e.DuAnId))
                .GroupBy(e => e.DuAnId)
                .Select(g => new { DuAnId = g.Key, Sum = g.Sum(x => x.GiaTri) })
                .ToDictionaryAsync(x => x.DuAnId, x => x.Sum, cancellationToken);

        var thanhToanDict = duAnIds.Count == 0 ? new Dictionary<Guid, long>()
            : await _thanhToan.GetQueryableSet()
                .AsNoTracking()
                .Where(e => !e.IsDeleted && duAnIds.Contains(e.DuAnId))
                .GroupBy(e => e.DuAnId)
                .Select(g => new { DuAnId = g.Key, Sum = g.Sum(x => (long)x.GiaTri.GetValueOrDefault()) })
                .ToDictionaryAsync(x => x.DuAnId, x => x.Sum, cancellationToken);

        // Map to DTOs
        var result = duAnList.Select(duAn => {
            var giaTriNghiemThu = nghiemThuDict.GetValueOrDefault(duAn.Id, 0);
            var giaTriGiaiNgan = thanhToanDict.GetValueOrDefault(duAn.Id, 0);

            var tenGiaiDoan = duAn.GiaiDoanHienTai?.Ten ?? "";
            var tenBuoc = duAn.BuocHienTai?.TenBuoc ?? "";
            var tienDo = string.IsNullOrEmpty(tenGiaiDoan) && string.IsNullOrEmpty(tenBuoc)
                ? null
                : $"{tenGiaiDoan}{(string.IsNullOrEmpty(tenGiaiDoan) || string.IsNullOrEmpty(tenBuoc) ? "" : " - ")}{tenBuoc}";

            return new BaoCaoDuAnDto {
                Id = duAn.Id,
                TenDuAn = duAn.TenDuAn,
                DonViPhuTrachChinhId = duAn.DonViPhuTrachChinhId,
                LoaiDuAnTheoNamId = duAn.LoaiDuAnTheoNamId,
                KhaiToanKinhPhi = duAn.KhaiToanKinhPhi,
                ThoiGianKhoiCong = duAn.ThoiGianKhoiCong,
                ThoiGianHoanThanh = duAn.ThoiGianHoanThanh,
                DuToanBanDau = duAn.SoDuToanCuoiCung,
                DuToanDieuChinh = duAn.SoDuToanCuoiCung,
                TienDo = tienDo,
                GiaTriNghiemThu = giaTriNghiemThu > 0 ? giaTriNghiemThu : null,
                GiaTriGiaiNgan = giaTriGiaiNgan > 0 ? giaTriGiaiNgan : null,
                HinhThucDauTuId = duAn.HinhThucDauTuId,
                LoaiDuAnId = duAn.LoaiDuAnId,
                NgayQuyetDinhDuToan = duAn.NgayQuyetDinhDuToan ?? duAn.NgayKyDuToan,
                SoQuyetDinhDuToan = duAn.SoQuyetDinhDuToan,
            };
        }).ToList();

        return new PaginatedList<BaoCaoDuAnDto>(
            result, totalCount,
            request.SearchDto.PageIndex,
            request.SearchDto.PageSize);
    }
}
