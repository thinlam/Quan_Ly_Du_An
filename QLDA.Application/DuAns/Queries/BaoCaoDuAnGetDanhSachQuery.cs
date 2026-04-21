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

    public BaoCaoDuAnGetDanhSachQueryHandler(IServiceProvider serviceProvider) {
        _duAn = serviceProvider.GetRequiredService<IRepository<DuAn, Guid>>();
        _duToan = serviceProvider.GetRequiredService<IRepository<DuToan, Guid>>();
        _nghiemThu = serviceProvider.GetRequiredService<IRepository<NghiemThu, Guid>>();
    }

    public async Task<PaginatedList<BaoCaoDuAnDto>> Handle(BaoCaoDuAnGetDanhSachQuery request,
        CancellationToken cancellationToken = default) {
        
        // Build the base query with filters
        var queryable = _duAn.GetQueryableSet().AsNoTracking()
            .Where(e => !e.IsDeleted)
            .Include(e => e.DuToans)
            .Include(e => e.BuocHienTai)
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
                .WhereIf(request.SearchDto.DonViPhuTrachChinhId > 0, 
                    e => e.DonViPhuTrachChinhId == request.SearchDto.DonViPhuTrachChinhId)
                .WhereIf(request.SearchDto.DonViPhuTrachChinhId == -1, 
                    e => e.DonViPhuTrachChinhId == null)
            );

        // Get total count before pagination
        var totalCount = await queryable.CountAsync(cancellationToken);

        // Get paginated DuAn data
        var duAnList = await queryable
            .Skip(request.SearchDto.Skip())
            .Take(request.SearchDto.Take())
            .ToListAsync(cancellationToken);

        // Build the result list using LINQ to Objects (in-memory)
        // First, get all DuToan and NghiemThu data for the projects
        var duAnIds = duAnList.Select(e => e.Id).ToList();
        
        var duToans = await _duToan.GetQueryableSet().AsNoTracking()
            .Where(e => !e.IsDeleted && duAnIds.Contains(e.DuAnId))
            .OrderBy(e => e.Id) // Order by ID to get first and last
            .ToListAsync(cancellationToken);

        var nghiemThus = await _nghiemThu.GetQueryableSet().AsNoTracking()
            .Where(e => !e.IsDeleted && duAnIds.Contains(e.DuAnId))
            .ToListAsync(cancellationToken);

        // Build the result DTOs
        var result = duAnList.Select(duAn => {
            // Get DuToan records for this project
            var duToanList = duToans.Where(dt => dt.DuAnId == duAn.Id).ToList();
            
            // Get initial budget (first DuToan)
            var duToanBanDau = duToanList.FirstOrDefault()?.SoDuToan;
            
            // Get adjusted/supplementary budget
            // If more than 1 record, get the last one; if exactly 1 record, return null
            long? duToanDieuChinh = null;
            if (duToanList.Count > 1) {
                duToanDieuChinh = duToanList.Last().SoDuToan;
            }

            // Get total acceptance value (sum of GiaTriNghiemThu)
            var giaTriNghiemThu = nghiemThus
                .Where(nt => nt.DuAnId == duAn.Id)
                .Sum(nt => nt.GiaTri);

            // Get implementation progress (current step name)
            var tenBuoc = duAn.BuocHienTai?.TenBuoc;

            return new BaoCaoDuAnDto {
                Id = duAn.Id,
                TenDuAn = duAn.TenDuAn,
                DonViPhuTrachChinhId = duAn.DonViPhuTrachChinhId,
                LoaiDuAnTheoNamId = duAn.LoaiDuAnTheoNamId,
                KhaiToanKinhPhi = duAn.KhaiToanKinhPhi,
                ThoiGianKhoiCong = duAn.ThoiGianKhoiCong,
                ThoiGianHoanThanh = duAn.ThoiGianHoanThanh,
                DuToanBanDau = duToanBanDau,
                DuToanDieuChinh = duToanDieuChinh,
                TienDo = tenBuoc,
                GiaTriNghiemThu = giaTriNghiemThu > 0 ? giaTriNghiemThu : null,
                HinhThucDauTuId = duAn.HinhThucDauTuId,
                LoaiDuAnId = duAn.LoaiDuAnId,
            };
        }).ToList();

        return new PaginatedList<BaoCaoDuAnDto>(result, totalCount, request.SearchDto.PageIndex, request.SearchDto.PageSize);
    }
}
