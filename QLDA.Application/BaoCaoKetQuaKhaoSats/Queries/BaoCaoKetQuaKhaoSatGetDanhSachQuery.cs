using Microsoft.EntityFrameworkCore;
using QLDA.Application.BaoCaoKetQuaKhaoSats.DTOs;
using QLDA.Application.Common.Mapping;
using QLDA.Application.TepDinhKems.DTOs;
using QLDA.Domain.Constants;
using QLDA.Domain.Interfaces;

namespace QLDA.Application.BaoCaoKetQuaKhaoSats.Queries;

public record BaoCaoKetQuaKhaoSatGetDanhSachQuery(BaoCaoKetQuaKhaoSatSearchDto SearchDto)
    : AggregateRootPagination, IMayHaveGlobalFilter, IRequest<PaginatedList<BaoCaoKetQuaKhaoSatDto>>
{
    public Guid? DuAnId { get; set; }
    public int? BuocId { get; set; }
    public string? GlobalFilter { get; set; }
}

internal class BaoCaoKetQuaKhaoSatGetDanhSachQueryHandler
    : IRequestHandler<BaoCaoKetQuaKhaoSatGetDanhSachQuery, PaginatedList<BaoCaoKetQuaKhaoSatDto>>
{
    private readonly IRepository<BaoCaoKetQuaKhaoSat, Guid> _repository;

    private readonly IRepository<TepDinhKem, Guid> _tepDinhKem;
    public BaoCaoKetQuaKhaoSatGetDanhSachQueryHandler(IServiceProvider serviceProvider)
    {
        _repository = serviceProvider.GetRequiredService<IRepository<BaoCaoKetQuaKhaoSat, Guid>>();
        _tepDinhKem = serviceProvider.GetRequiredService<IRepository<TepDinhKem, Guid>>();
    }

    public async Task<PaginatedList<BaoCaoKetQuaKhaoSatDto>> Handle(
        BaoCaoKetQuaKhaoSatGetDanhSachQuery request, CancellationToken cancellationToken = default) {
        var queryable = _repository.GetQueryableSet()
            .AsNoTracking()
            .Where(e => !e.IsDeleted)
            .Include(e => e.TrangThai)
            .WhereIf(request.SearchDto.DuAnId.HasValue, e => e.DuAnId == request.SearchDto.DuAnId)
            .WhereIf(request.SearchDto.BuocId.HasValue, e => e.BuocId == request.SearchDto.BuocId)
            .WhereGlobalFilter(
                request,
                e => e.NoiDungBaoCao,
                e => e.NoiDungNghiemThu);
        var zone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");

        var data = await queryable.ToListAsync(cancellationToken);
        return await queryable
            .Select(e => new BaoCaoKetQuaKhaoSatDto(){
                Id = e.Id,
                DuAnId = e.DuAnId,
                BuocId = e.BuocId,
                NoiDungBaoCao = e.NoiDungBaoCao,
                NoiDungNghiemThu = e.NoiDungNghiemThu,
                NgayKhaoSat = DateOnly.FromDateTime(
                    TimeZoneInfo.ConvertTime(e.NgayKhaoSat, zone).DateTime),
                TenTrangThai = e.TrangThai != null && e.TrangThai.Ma != "LEG" ? e.TrangThai.Ten : TrangThaiPheDuyetCodes.Default.TenDuThao,
                TrangThaiId = e.TrangThaiId,
                MaTrangThai = e.TrangThai != null && e.TrangThai.Ma != "LEG" ? e.TrangThai.Ma : TrangThaiPheDuyetCodes.Default.DuThao,
                
                DanhSachTepDinhKem = _tepDinhKem.GetQueryableSet()
                   .Where(i => i.GroupId == e.Id.ToString())
                   .Select(i => i.ToDto()).ToList(),
            })
            .PaginatedListAsync(request.Skip(), request.Take(), cancellationToken);
    }
}
