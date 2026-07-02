using Microsoft.EntityFrameworkCore;
using QLDA.Application.Common.Mapping;
using QLDA.Application.DuongDiTrangThaiToTrinhs.DTOs;
using QLDA.Application.HoSoDeXuatCapDoCntts.DTOs;
using QLDA.Application.TepDinhKems.DTOs;
using QLDA.Application.Common.Constants;
using QLDA.Domain.Constants;

namespace QLDA.Application.HoSoDeXuatCapDoCntts.Queries;

public record HoSoDeXuatCapDoCnttGetDanhSachQuery(HoSoDeXuatCapDoCnttSearchDto SearchDto)
    : AggregateRootPagination, IMayHaveGlobalFilter, IRequest<PaginatedList<HoSoDeXuatCapDoCnttDto>> {
    public string? GlobalFilter { get; set; }
}

internal class HoSoDeXuatCapDoCnttGetDanhSachQueryHandler 
    : IRequestHandler<HoSoDeXuatCapDoCnttGetDanhSachQuery, PaginatedList<HoSoDeXuatCapDoCnttDto>> {
    
    private readonly IRepository<HoSoDeXuatCapDoCntt, Guid> HoSoDeXuatCapDoCntt;
    private readonly IRepository<TepDinhKem, Guid> TepDinhKem;
    private readonly IRepository<DuongDiTrangThaiToTrinh, long> _duongDiTrangThaiToTrinh ;

    public HoSoDeXuatCapDoCnttGetDanhSachQueryHandler(IServiceProvider serviceProvider) {
        HoSoDeXuatCapDoCntt = serviceProvider.GetRequiredService<IRepository<HoSoDeXuatCapDoCntt, Guid>>();
        TepDinhKem = serviceProvider.GetRequiredService<IRepository<TepDinhKem, Guid>>();
        _duongDiTrangThaiToTrinh = serviceProvider.GetRequiredService<IRepository<DuongDiTrangThaiToTrinh, long>>();
    }

    public async Task<PaginatedList<HoSoDeXuatCapDoCnttDto>> Handle(HoSoDeXuatCapDoCnttGetDanhSachQuery request,
        CancellationToken cancellationToken = default) {
        var duongDi = await _duongDiTrangThaiToTrinh.GetQueryableSet().AsNoTracking()
                  .Where(x => x.Used && !(x.IsDeleted ?? false))
                    .Where(x => x.Loai == PheDuyetEntityNames.HoSoDeXuatCapDoCntt)
                  .ToListAsync(cancellationToken);
        var duongDiLookup = duongDi
            .Where(x => !string.IsNullOrWhiteSpace(x.MaTrangThaiHienTai))
            .GroupBy(x => x.MaTrangThaiHienTai!)
            .ToDictionary(
                g => g.Key,
                g => g.Select(x => new DuongDiTrangThaiToTrinhDto
                {
                    MaTrangThaiHienTai = x.MaTrangThaiHienTai,
                    MaTrangThaiTiepTheo = x.MaTrangThaiTiepTheo,
                    TenTrangThaiTiepTheo = x.TenTrangThaiTiepTheo,
                    RoleId = x.RoleId,
                    RoleLevel = x.RoleLevel
                }).ToList()
        );

        var queryable = HoSoDeXuatCapDoCntt.GetQueryableSet()
            .AsNoTracking()
            .Include(e => e.CapDo).Include(x => x.TrangThai)
            .WhereIf(request.SearchDto.DuAnId.HasValue, e => e.DuAnId == request.SearchDto.DuAnId)
            .WhereIf(request.SearchDto.LoaiDuAnTheoNamId > 0, e => e.DuAn!.LoaiDuAnTheoNamId == request.SearchDto.LoaiDuAnTheoNamId)
            .WhereIf(request.SearchDto.BuocId.HasValue, e => e.BuocId == request.SearchDto.BuocId)
            .WhereGlobalFilter(
                request,
                e => e.NoiDungDeNghi,
                e => e.NoiDungBaoCao,
                e => e.NoiDungDuThao
            );

        var dtos = await queryable
            .Select(e => e.ToDto())
            .PaginatedListAsync(request.Skip(), request.Take(), cancellationToken);

        // Load file đính kèm cho từng item
        var groupIds = dtos.Data.Select(d => d.Id.ToString()).ToList();
        if (groupIds.Count > 0) {
            var files = await TepDinhKem.GetQueryableSet()
                .AsNoTracking()
                .Where(f => groupIds.Contains(f.GroupId))
                .ToListAsync(cancellationToken);

            foreach (var dto in dtos.Data) {
                dto.DanhSachTepDinhKem = files.Where(f => f.GroupId == dto.Id.ToString())
                    .Select(f => f.ToDto()).ToList();
            }
        }
        foreach (var item in dtos.Data)
        {
            item.ThaoTacTiepTheo =  !string.IsNullOrEmpty(item.MaTrangThai)
                && duongDiLookup.TryGetValue(item.MaTrangThai.Trim(), out var actions)
                    ? actions  : [];
        }
        return dtos;
    }
}