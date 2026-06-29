using Microsoft.EntityFrameworkCore;
using QLDA.Application.Common.Mapping;
using QLDA.Application.QuyetDinhDuyetKHLCNTs.DTOs;
using QLDA.Application.TepDinhKems.DTOs;
using QLDA.Application.TongHopVanBanQuyetDinhs.DTOs;

namespace QLDA.Application.QuyetDinhDuyetKHLCNTs.Queries;

public record QuyetDinhDuyetKHLCNTGetDanhSachQuery : AggregateRootPagination, IMayHaveGlobalFilter, IRequest<PaginatedList<QuyetDinhDuyetKHLCNTDto>> {
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
    QuyetDinhDuyetKHLCNTGetDanhSachQueryHandler : IRequestHandler<QuyetDinhDuyetKHLCNTGetDanhSachQuery,
    PaginatedList<QuyetDinhDuyetKHLCNTDto>> {
    private readonly IRepository<QuyetDinhDuyetKHLCNT, Guid> QuyetDinhDuyetKHLCNT;
    private readonly IRepository<TepDinhKem, Guid> TepDinhKem;

    public QuyetDinhDuyetKHLCNTGetDanhSachQueryHandler(IServiceProvider serviceProvider) {
        QuyetDinhDuyetKHLCNT = serviceProvider.GetRequiredService<IRepository<QuyetDinhDuyetKHLCNT, Guid>>();
        TepDinhKem = serviceProvider.GetRequiredService<IRepository<TepDinhKem, Guid>>();
    }

    public async Task<PaginatedList<QuyetDinhDuyetKHLCNTDto>> Handle(QuyetDinhDuyetKHLCNTGetDanhSachQuery request,
        CancellationToken cancellationToken = default) {
        var queryable = QuyetDinhDuyetKHLCNT.GetQueryableSet().AsNoTracking()
            .Where(e => !e.VanBanQuyetDinh.DuAn!.IsDeleted)
            .WhereIf(request.DuAnId != null, e => e.VanBanQuyetDinh.DuAnId == request.DuAnId)
            .WhereIf(request.LoaiDuAnTheoNamId > 0, e => e.VanBanQuyetDinh.DuAn!.LoaiDuAnTheoNamId == request.LoaiDuAnTheoNamId)
            .WhereIf(request.BuocId > 0, e => e.VanBanQuyetDinh.BuocId == request.BuocId)
            .WhereGlobalFilter(
                request,
                e => e.VanBanQuyetDinh.So,
                e => e.VanBanQuyetDinh.NguoiKy
            );

        return await queryable
            .Select(e => new QuyetDinhDuyetKHLCNTDto() {
                Id = e.Id,
                KeHoachLuaChonNhaThauId = e.KeHoachLuaChonNhaThauId,
                VanBanQuyetDinh = new VanBanQuyetDinhDto
                {
                    TableName = e.VanBanQuyetDinh.Loai,
                    DuAnId = e.VanBanQuyetDinh.DuAnId,
                    BuocId = e.VanBanQuyetDinh.BuocId,
                    So = e.VanBanQuyetDinh.So,
                    Ngay = e.VanBanQuyetDinh.Ngay,
                    CoQuanQuyetDinh = e.VanBanQuyetDinh.CoQuanQuyetDinh,
                    TrichYeu = e.VanBanQuyetDinh.TrichYeu,
                    NgayKy = e.VanBanQuyetDinh.NgayKy,
                    NguoiKy = e.VanBanQuyetDinh.NguoiKy,
                },
                DanhSachTepDinhKem = TepDinhKem.GetQueryableSet()
                    .Where(i => i.GroupId == e.Id.ToString())
                    .Select(i => i.ToDto()).ToList(),
            })
            .PaginatedListAsync(request.Skip(), request.Take(), cancellationToken: cancellationToken);
    }
}