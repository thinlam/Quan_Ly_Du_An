using Microsoft.EntityFrameworkCore;
using QLDA.Application.Common.Mapping;
using QLDA.Application.TepDinhKems.DTOs;
using QLDA.Application.PheDuyetDuToans.DTOs;

namespace QLDA.Application.PheDuyetDuToans.Queries;

public record PheDuyetDuToanGetDanhSachQuery : AggregateRootPagination, IMayHaveGlobalFilter, IRequest<PaginatedList<PheDuyetDuToanDto>> {
    public Guid? DuAnId { get; set; }
    public int? BuocId { get; set; }
    public string? GlobalFilter { get; set; }
    public bool IsNoTracking { get; set; }
}

internal class
    PheDuyetDuToanGetDanhSachQueryHandler : IRequestHandler<PheDuyetDuToanGetDanhSachQuery,
    PaginatedList<PheDuyetDuToanDto>> {
    private readonly IRepository<PheDuyetDuToan, Guid> PheDuyetDuToan;
    private readonly IRepository<TepDinhKem, Guid> TepDinhKem;

    public PheDuyetDuToanGetDanhSachQueryHandler(IServiceProvider serviceProvider) {
        PheDuyetDuToan = serviceProvider.GetRequiredService<IRepository<PheDuyetDuToan, Guid>>();
        TepDinhKem = serviceProvider.GetRequiredService<IRepository<TepDinhKem, Guid>>();
    }


    public async Task<PaginatedList<PheDuyetDuToanDto>> Handle(PheDuyetDuToanGetDanhSachQuery request,
        CancellationToken cancellationToken = default) {
        var queryable = PheDuyetDuToan.GetQueryableSet().AsNoTracking()
            .Where(e => !e.IsDeleted)
            .Where(e => !e.DuAn!.IsDeleted)
            .WhereIf(request.DuAnId != null, e => e.DuAnId == request.DuAnId)
            .WhereIf(request.BuocId > 0, e => e.BuocId == request.BuocId)
            .WhereGlobalFilter(
                request,
                e => e.So,
                e => e.NguoiKy,
                e => e.ChucVu!.Ten,
                e => e.TrichYeu
            );

        return await queryable
            .Select(e => new PheDuyetDuToanDto() {
                Id = e.Id,
                ChucVuId = e.ChucVuId,
                BuocId = e.BuocId,
                DuAnId = e.DuAnId,
                NgayKy = e.NgayKy,
                NguoiKy = e.NguoiKy,
                SoVanBan = e.So,
                GiaTriDuThau = e.GiaTriDuThau,
                TrichYeu = e.TrichYeu,
                TrangThaiId = e.TrangThaiId,
                TenTrangThai = e.TrangThai != null ? e.TrangThai.Ten : null,
                MaTrangThai = e.TrangThai != null ? e.TrangThai.Ma : null,
                DanhSachTepDinhKem = TepDinhKem.GetQueryableSet()
                    .Where(i => i.GroupId == e.Id.ToString())
                    .Select(i => i.ToDto()).ToList(),
            })
            .PaginatedListAsync(request.Skip(), request.Take(), cancellationToken: cancellationToken);
    }
}