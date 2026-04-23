using Microsoft.EntityFrameworkCore;
using QLDA.Application.Common.Mapping;
using QLDA.Application.DuAns.DTOs;

namespace QLDA.Application.DuAns.Queries;

public record BaoCaoDuAnGetDanhSachQuery(BaoCaoDuAnSearchDto SearchDto) : AggregateRootPagination, IRequest<PaginatedList<BaoCaoDuAnDto>> {
    public bool IsNoTracking { get; set; } = true;
}

internal class BaoCaoDuAnGetDanhSachQueryHandler : IRequestHandler<BaoCaoDuAnGetDanhSachQuery, PaginatedList<BaoCaoDuAnDto>> {
    private readonly IRepository<DuAn, Guid> _duAn;
    private readonly IRepository<DuToan, Guid> _duToan;
    private readonly IRepository<NghiemThu, Guid> _nghiemThu;
    private readonly IRepository<ThanhToan, Guid> _thanhToan;

    public BaoCaoDuAnGetDanhSachQueryHandler(IServiceProvider serviceProvider) {
        _duAn = serviceProvider.GetRequiredService<IRepository<DuAn, Guid>>();
        _duToan = serviceProvider.GetRequiredService<IRepository<DuToan, Guid>>();
        _nghiemThu = serviceProvider.GetRequiredService<IRepository<NghiemThu, Guid>>();
        _thanhToan = serviceProvider.GetRequiredService<IRepository<ThanhToan, Guid>>();
    }

    public async Task<PaginatedList<BaoCaoDuAnDto>> Handle(BaoCaoDuAnGetDanhSachQuery request,
        CancellationToken cancellationToken = default) {
        
        // Build the base query with filters
        var queryable = _duAn.GetQueryableSet().AsNoTracking()
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
            .WhereIf(request.SearchDto.DonViPhuTrachChinhId > 0, 
                e => e.DonViPhuTrachChinhId == request.SearchDto.DonViPhuTrachChinhId);

        // Get total count before pagination
        var totalCount = await queryable.CountAsync(cancellationToken);

        // Get paginated DuAn data
        var duAnList = await queryable
            .Skip(request.SearchDto.Skip())
            .Take(request.SearchDto.Take())
            .ToListAsync(cancellationToken);

        // Build the result list using LINQ to Objects (in-memory)
        // Get all NghiemThu data for the projects
        var duAnIds = duAnList.Select(e => e.Id).ToList();
        
        var nghiemThuDict = await _nghiemThu.GetQueryableSet()
            .AsNoTracking()
            .Where(e => !e.IsDeleted && duAnIds.Contains(e.DuAnId))
            .GroupBy(e => e.DuAnId)
            .Select(g => new {
                DuAnId = g.Key,
                Sum = g.Sum(x => x.GiaTri)
            })
            .ToDictionaryAsync(x => x.DuAnId, x => x.Sum, cancellationToken);

        // Get all ThanhToan data for the projects (Giải ngân)
        var thanhToanDict = await _thanhToan.GetQueryableSet()
            .AsNoTracking()
            .Where(e => !e.IsDeleted && duAnIds.Contains(e.DuAnId))
            .GroupBy(e => e.DuAnId)
            .Select(g => new {
                DuAnId = g.Key,
                Sum = g.Sum(x => x.GiaTri)
            })
            .ToDictionaryAsync(x => x.DuAnId, x => x.Sum, cancellationToken);

        // Build the result DTOs
        var result = duAnList.Select(duAn => {
            // Get total acceptance value (sum of GiaTriNghiemThu)
            var giaTriNghiemThu = nghiemThuDict.ContainsKey(duAn.Id) ? nghiemThuDict[duAn.Id] : 0;
            
            // Get total disbursement value (sum of GiaTriGiaiNgan)
            var giaTriGiaiNgan = thanhToanDict.ContainsKey(duAn.Id) ? thanhToanDict[duAn.Id] : 0;

            // Get implementation progress (phase name - step name)
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
                NgayQuyetDinhDuToan = duAn.NgayKyDuToan,
                SoQuyetDinhDuToan = duAn.SoQuyetDinhDuToan,
            };
        }).ToList();

        return new PaginatedList<BaoCaoDuAnDto>(result, totalCount, request.SearchDto.PageIndex, request.SearchDto.PageSize);
    }
}
