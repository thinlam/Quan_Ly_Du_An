using Microsoft.EntityFrameworkCore;
using QLDA.Application.Common.Mapping;
using QLDA.Application.TepDinhKems.DTOs;
using QLDA.Application.QuyetDinhLapBanQLDAs.DTOs;
using QLDA.Application.Common.Interfaces;

namespace QLDA.Application.QuyetDinhLapBanQLDAs.Queries;

public record QuyetDinhLapBanQldaGetDanhSachQuery : AggregateRootPagination, IMayHaveGlobalFilter, IFromDateToDate, IRequest<PaginatedList<QuyetDinhLapBanQldaDto>> {
    public Guid? DuAnId { get; set; }
    public int? BuocId { get; set; }
    public string? GlobalFilter { get; set; }
    public bool IsNoTracking { get; set; }

    public string? SoQuyetDinh { get; set; }
    public DateOnly? TuNgay { get; set; }
    public DateOnly? DenNgay { get; set; }
    /// <summary>
    /// Loại dự án theo năm - tài chính
    /// </summary>
    /// <remarks>PMIS #9609</remarks>
    public int? LoaiDuAnTheoNamId { get; set; }
}

internal class
    QuyetDinhLapBanQldaGetDanhSachQueryHandler : IRequestHandler<QuyetDinhLapBanQldaGetDanhSachQuery,
    PaginatedList<QuyetDinhLapBanQldaDto>> {
    private readonly IRepository<QuyetDinhLapBanQLDA, Guid> QuyetDinhLapBanQLDA;
    private readonly IRepository<TepDinhKem, Guid> TepDinhKem;

    public QuyetDinhLapBanQldaGetDanhSachQueryHandler(IServiceProvider serviceProvider) {
        QuyetDinhLapBanQLDA = serviceProvider.GetRequiredService<IRepository<QuyetDinhLapBanQLDA, Guid>>();
        TepDinhKem = serviceProvider.GetRequiredService<IRepository<TepDinhKem, Guid>>();
    }

    public async Task<PaginatedList<QuyetDinhLapBanQldaDto>> Handle(QuyetDinhLapBanQldaGetDanhSachQuery request,
        CancellationToken cancellationToken = default) {
        var queryable = QuyetDinhLapBanQLDA.GetQueryableSet().AsNoTracking()
            .Where(e => !e.DuAn!.IsDeleted)
            .WhereIf(request.DuAnId != null, e => e.DuAnId == request.DuAnId)
            .WhereIf(request.LoaiDuAnTheoNamId > 0, e => e.DuAn!.LoaiDuAnTheoNamId == request.LoaiDuAnTheoNamId)
            .WhereIf(request.BuocId > 0, e => e.BuocId == request.BuocId)
            .WhereIf(request.SoQuyetDinh.IsNotNullOrWhitespace(), e => e.So!.ToLower().Contains(request.SoQuyetDinh!.ToLower()))
            .WhereIf(request.TuNgay.HasValue, e => e.Ngay.HasValue && e.Ngay.Value >= request.TuNgay!.Value.ToStartOfDayUtc())
            .WhereIf(request.DenNgay.HasValue, e => e.Ngay.HasValue && e.Ngay.Value <= request.DenNgay!.Value.ToEndOfDayUtc())
            .WhereGlobalFilter(
                request,
                e => e.So,
                e => e.TrichYeu,
                e => e.NguoiKy
            );

        return await queryable
            .Select(e => new QuyetDinhLapBanQldaDto() {
                Id = e.Id,
                DuAnId = e.DuAnId,
                BuocId = e.BuocId == 0 ? null : e.BuocId,
                SoQuyetDinh = e.So,
                NgayQuyetDinh = e.Ngay,
                SoDuThao= e.SoDuThao,
                TrangThaiId = e.TrangThaiId,
                MaTrangThai = e.TrangThai!.Ma ,
                TenTrangThai = e.TrangThai!.Ten ?? string.Empty,
                TrichYeu = e.TrichYeu,
                NguoiKy = e.NguoiKy,
                NgayKy = e.NgayKy,
                DanhSachTepDinhKem = TepDinhKem.GetQueryableSet()
                    .Where(i => i.GroupId == e.Id.ToString())
                    .Select(i => i.ToDto()).ToList(),
            })
            .PaginatedListAsync(request.Skip(), request.Take(), cancellationToken: cancellationToken);
    }
}