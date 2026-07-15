using Microsoft.EntityFrameworkCore;
using QLDA.Application.Common.Interfaces;
using QLDA.Application.Common.Mapping;
using QLDA.Application.QuyetDinhDuyetDuToanDtos.DTOs;
using QLDA.Application.TepDinhKems.DTOs;

namespace QLDA.Application.QuyetDinhDuyetDuToans.Queries;

public record QuyetDinhDuyetDuToanGetDanhSachQuery : AggregateRootPagination, IMayHaveGlobalFilter, IFromDateToDate, IRequest<PaginatedList<QuyetDinhDuyetDuToanDto>> {
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
    QuyetDinhDuyetDuToanGetDanhSachQueryHandler : IRequestHandler<QuyetDinhDuyetDuToanGetDanhSachQuery,
    PaginatedList<QuyetDinhDuyetDuToanDto>> {
    private readonly IRepository<QuyetDinhDuyetDuToan, Guid> QuyetDinhDuyetDuToan;
    private readonly IRepository<Attachment, Guid> TepDinhKem;

    public QuyetDinhDuyetDuToanGetDanhSachQueryHandler(IServiceProvider serviceProvider) {
        QuyetDinhDuyetDuToan = serviceProvider.GetRequiredService<IRepository<QuyetDinhDuyetDuToan, Guid>>();
        TepDinhKem = serviceProvider.GetRequiredService<IRepository<Attachment, Guid>>();
    }

    public async Task<PaginatedList<QuyetDinhDuyetDuToanDto>> Handle(QuyetDinhDuyetDuToanGetDanhSachQuery request,
        CancellationToken cancellationToken = default) {
        var queryable = QuyetDinhDuyetDuToan.GetQueryableSet().AsNoTracking()
            .Include(e => e.DuAn).Include(e => e.HinhThucQuanLyDuAn).Include(e => e.KeHoachLuaChonNhaThau).Include(e => e.TrangThai)
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
                e => e.TrichYeu
            );

        return await queryable
            .Select(e => new QuyetDinhDuyetDuToanDto() {
                Id = e.Id,
                DuAnId = e.DuAnId,
                BuocId = e.BuocId == 0 ? null : e.BuocId,
                SoQuyetDinh = e.So,
                NgayQuyetDinh = e.Ngay,
                TrichYeu = e.TrichYeu,
                TrangThaiId  = e.TrangThaiId,
                TenTrangThai = e.TrangThai!.Ten ?? string.Empty,
                GiaTri = e.GiaTri,
                TenHinhThucQuanLy = e.HinhThucQuanLyDuAn!.Ten ?? string.Empty,
                TenDuAn = e.DuAn!.TenDuAn ?? string.Empty,
                TenKeHoachLuaChonNhaThau = e.KeHoachLuaChonNhaThau!.Ten ?? string.Empty,
                DanhSachTepDinhKem = TepDinhKem.GetQueryableSet()
                    .Where(i => i.GroupId == e.Id.ToString())
                    .Select(i => i.ToDto()).ToList(),
            })
            .PaginatedListAsync(request.Skip(), request.Take(), cancellationToken: cancellationToken);
    }
}