using Microsoft.EntityFrameworkCore;
using QLDA.Application.Common.Mapping;
using QLDA.Application.TepDinhKems.DTOs;
using QLDA.Application.QuyetDinhDuyetQuyetToans.DTOs;

namespace QLDA.Application.QuyetDinhDuyetQuyetToans.Queries;

public record QuyetDinhDuyetQuyetToanGetDanhSachQuery : AggregateRootPagination, IMayHaveGlobalFilter, IRequest<PaginatedList<QuyetDinhDuyetQuyetToanDto>> {
    public Guid? DuAnId { get; set; }
    public int? BuocId { get; set; }
    public string? GlobalFilter { get; set; }
    public bool IsNoTracking { get; set; }
    /// <summary>
    /// Loại dự án theo năm - tài chính
    /// </summary>
    /// <remarks>PMIS #9609</remarks>
    public int? LoaiDuAnTheoNamId { get; set; }
}

internal class
    QuyetDinhDuyetQuyetToanGetDanhSachQueryHandler : IRequestHandler<QuyetDinhDuyetQuyetToanGetDanhSachQuery,
    PaginatedList<QuyetDinhDuyetQuyetToanDto>> {
    private readonly IRepository<QuyetDinhDuyetQuyetToan, Guid> QuyetDinhDuyetQuyetToan;
    private readonly IRepository<TepDinhKem, Guid> TepDinhKem;

    public QuyetDinhDuyetQuyetToanGetDanhSachQueryHandler(IServiceProvider serviceProvider) {
        QuyetDinhDuyetQuyetToan = serviceProvider.GetRequiredService<IRepository<QuyetDinhDuyetQuyetToan, Guid>>();
        TepDinhKem = serviceProvider.GetRequiredService<IRepository<TepDinhKem, Guid>>();
    }

    public async Task<PaginatedList<QuyetDinhDuyetQuyetToanDto>> Handle(QuyetDinhDuyetQuyetToanGetDanhSachQuery request,
        CancellationToken cancellationToken = default) {
        var queryable = QuyetDinhDuyetQuyetToan.GetQueryableSet().AsNoTracking()
            .Where(e => !e.DuAn!.IsDeleted)
            .WhereIf(request.DuAnId != null, e => e.DuAnId == request.DuAnId)
            .WhereIf(request.LoaiDuAnTheoNamId > 0, e => e.DuAn!.LoaiDuAnTheoNamId == request.LoaiDuAnTheoNamId)
            .WhereIf(request.BuocId > 0, e => e.BuocId == request.BuocId)
            .WhereGlobalFilter(
                request,
                e => e.So,
                e => e.NguoiKy
            );

        return await queryable
            .Select(e => new QuyetDinhDuyetQuyetToanDto() {
                Id = e.Id,
                DuAnId = e.DuAnId,
                BuocId = e.BuocId,
                SoQuyetDinh = e.So,
                NgayQuyetDinh = e.Ngay,
                CoQuanQuyetDinh = e.CoQuanQuyetDinh,
                TrichYeu = e.TrichYeu,
                NgayKy = e.NgayKy,
                NguoiKy = e.NguoiKy,
                GiaTri = e.GiaTri,
                DanhSachTepDinhKem = TepDinhKem.GetQueryableSet()
                    .Where(i => i.GroupId == e.Id.ToString())
                    .Select(i => i.ToDto()).ToList(),
            })
            .PaginatedListAsync(request.Skip(), request.Take(), cancellationToken: cancellationToken);
    }
}