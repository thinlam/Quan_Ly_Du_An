using Microsoft.EntityFrameworkCore;
using QLDA.Application.Common.Mapping;
using QLDA.Application.TepDinhKems.DTOs;
using QLDA.Application.ChuTruongLapKeHoachs.DTOs;
using QLDA.Domain.Constants;

namespace QLDA.Application.ChuTruongLapKeHoachs.Queries;

public record ChuTruongLapKeHoachDanhSachQuery : AggregateRootPagination, IMayHaveGlobalFilter, IRequest<PaginatedList<ChuTruongLapKeHoachDto>> {
    public Guid? DuAnId { get; set; }
    public string? GlobalFilter { get; set; }
    public bool IsNoTracking { get; set; }
    public DateOnly? TuNgay { get; set; }
    public DateOnly? DenNgay { get; set; }
    public string? So { get; set; }
}

internal class
    ChuTruongLapKeHoachDanhSachQueryHandler : IRequestHandler<ChuTruongLapKeHoachDanhSachQuery,
    PaginatedList<ChuTruongLapKeHoachDto>> {
    private readonly IRepository<ChuTruongLapKeHoach, Guid> ChuTruongLapKeHoach;
    private readonly IRepository<TepDinhKem, Guid> TepDinhKem;

    public ChuTruongLapKeHoachDanhSachQueryHandler(IServiceProvider serviceProvider) {
        ChuTruongLapKeHoach = serviceProvider.GetRequiredService<IRepository<ChuTruongLapKeHoach, Guid>>();
        TepDinhKem = serviceProvider.GetRequiredService<IRepository<TepDinhKem, Guid>>();
    }

    public async Task<PaginatedList<ChuTruongLapKeHoachDto>> Handle(ChuTruongLapKeHoachDanhSachQuery request,
        CancellationToken cancellationToken = default) {
        DateTimeOffset? tuNgay = request.TuNgay.HasValue
             ? new DateTimeOffset(request.TuNgay.Value.ToDateTime(TimeOnly.MinValue))
             : null;

        DateTimeOffset? denNgay = request.DenNgay.HasValue
            ? new DateTimeOffset(request.DenNgay.Value.ToDateTime(TimeOnly.MaxValue))
            : null;
        var queryable = ChuTruongLapKeHoach.GetQueryableSet().AsNoTracking()
            .Where(e => !e.IsDeleted)
            .Where(e => !e.DuAn!.IsDeleted)
            .WhereIf(request.TuNgay != null, e => e.NgayToTrinh >= tuNgay)
            .WhereIf(request.DenNgay != null, e => e.NgayToTrinh <= denNgay)
            .WhereIf(request.DuAnId != null, e => e.DuAnId == request.DuAnId)
            .WhereGlobalFilter(
                request,
                e => e.SoToTrinh
            );

        return await queryable
            .Select(e => new ChuTruongLapKeHoachDto() {
                Id = e.Id,
                DuAnId = e.DuAnId,
                BuocId = e.BuocId,
                TenDuAn = e.DuAn != null ? e.DuAn.TenDuAn : "Không rõ",
                TrangThaiId = e.TrangThaiId,
                TenTrangThai = e.TrangThai != null ? e.TrangThai.Ten :string.Empty,

                LoaiDeXuat = e.LoaiDeXuat ,
                TenLoaiDeXuat = e.LoaiDeXuat == (int)LoaiDeXuatLCNTonstants.LoaiDeXuatMacDinh.KhongLap ? 
                LoaiDeXuatLCNTonstants.Default.KhongLap : LoaiDeXuatLCNTonstants.Default.XinChuTruong,
                SoToTrinh= e.SoToTrinh,
                NgayToTrinh = e.NgayToTrinh,
                TrichYeu = e.TrichYeu,
                ButPhe = e.ButPhe,
                DanhSachTepDinhKem = TepDinhKem.GetQueryableSet()
                    .Where(i => i.GroupId == e.Id.ToString())
                    .Select(i => i.ToDto()).ToList(),
            })
            .PaginatedListAsync(request.Skip(), request.Take(), cancellationToken: cancellationToken);
    }
}