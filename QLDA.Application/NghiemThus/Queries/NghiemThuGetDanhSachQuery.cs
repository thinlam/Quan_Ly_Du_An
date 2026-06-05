using Microsoft.EntityFrameworkCore;
using QLDA.Application.Common.Mapping;
using QLDA.Application.TepDinhKems.DTOs;
using QLDA.Application.NghiemThus.DTOs;

namespace QLDA.Application.NghiemThus.Queries;

public record NghiemThuGetDanhSachQuery : AggregateRootPagination, IMayHaveGlobalFilter, IRequest<PaginatedList<NghiemThuDto>> {
    public Guid? DuAnId { get; set; }
    public int? BuocId { get; set; }
    public Guid? HopDongId { get; set; }
    public Guid? ThanhToanId { get; set; }
    public string? GlobalFilter { get; set; }
    public bool IsNoTracking { get; set; }
    public bool IsCbo { get; set; }
}

internal class
    NghiemThuGetDanhSachQueryHandler : IRequestHandler<NghiemThuGetDanhSachQuery,
    PaginatedList<NghiemThuDto>> {
    private readonly IRepository<NghiemThu, Guid> NghiemThu;
    private readonly IRepository<TepDinhKem, Guid> TepDinhKem;

    public NghiemThuGetDanhSachQueryHandler(IServiceProvider serviceProvider) {
        NghiemThu = serviceProvider.GetRequiredService<IRepository<NghiemThu, Guid>>();
        TepDinhKem = serviceProvider.GetRequiredService<IRepository<TepDinhKem, Guid>>();
    }

    public async Task<PaginatedList<NghiemThuDto>> Handle(NghiemThuGetDanhSachQuery request,
        CancellationToken cancellationToken = default) {
        var queryable = NghiemThu.GetQueryableSet().AsNoTracking()
            .Where(e => !e.DuAn!.IsDeleted)
            
            .WhereIf(request.DuAnId != null, e => e.DuAnId == request.DuAnId)
            .WhereIf(request.HopDongId != null, e => e.HopDongId == request.HopDongId)
            .WhereIf(request.BuocId > 0, e => e.BuocId == request.BuocId)
            .WhereGlobalFilter(
                request,
                e => e.SoBienBan,
                e => e.NoiDung
            )
            .WhereFunc(request.IsCbo,
                q => q
                    .WhereIf(request.ThanhToanId.HasValue, e => e.ThanhToan!.Id == request.ThanhToanId || e.ThanhToan == null),
                q => q
                    .WhereIf(request.ThanhToanId.HasValue, e => e.ThanhToan!.Id == request.ThanhToanId)

            );

        return await queryable
            .Select(e => new NghiemThuDto() {
                Id = e.Id,
                DuAnId = e.DuAnId,
                BuocId = e.BuocId,
                HopDongId = e.HopDongId,
                PhuLucHopDongIds = e.NghiemThuPhuLucHopDongs!.Select(junction => junction.RightId).ToList(),
                Dot = e.Dot,
                Ngay = e.Ngay,
                NoiDung = e.NoiDung,
                SoBienBan = e.SoBienBan,
                ThanhToanId = !e.ThanhToan!.IsDeleted ? e.ThanhToan!.Id : null,
                DanhSachTepDinhKem = TepDinhKem.GetQueryableSet()
                    .Where(i => i.GroupId == e.Id.ToString())
                    .Select(i => i.ToDto()).ToList(),
            })
            .PaginatedListAsync(request.Skip(), request.Take(), cancellationToken: cancellationToken);
    }
}