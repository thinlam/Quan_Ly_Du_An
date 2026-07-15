using Microsoft.EntityFrameworkCore;
using QLDA.Application.Common.Interfaces;
using QLDA.Application.Common.Mapping;
using QLDA.Application.TepDinhKems.DTOs;
using QLDA.Application.DeXuatNhuCauKinhPhiNams.DTOs;

namespace QLDA.Application.DeXuatNhuCauKinhPhiNams.Queries;

public record DeXuatNhuCauKinhPhiNamQuery : AggregateRootPagination, IMayHaveGlobalFilter, IFromDateToDate, IRequest<PaginatedList<DeXuatNhuCauKinhPhiNamDto>> {
 
    public bool IsNoTracking { get; set; }
    public string? GlobalFilter { get; set; }
    public long? PhongBanDeXuatId { get; set; }
    public long? NguoiDeXuatId { get; set; }
    public string? So { get; set; }
    public string? TrichYeu { get; set; }
    public DateOnly? TuNgay { get; set; }
    public DateOnly? DenNgay { get; set; }
    public int? TrangThaiId { get; set; }

}

internal class
    DeXuatNhuCauKinhPhiNamQueryHandler(IServiceProvider ServiceProvider)
    : IRequestHandler<DeXuatNhuCauKinhPhiNamQuery, PaginatedList<DeXuatNhuCauKinhPhiNamDto>> {
    private readonly IRepository<DeXuatNhuCauKinhPhiNam, Guid> DeXuatNhuCauKinhPhiNam =
        ServiceProvider.GetRequiredService<IRepository<DeXuatNhuCauKinhPhiNam, Guid>>();

    private readonly IRepository<Attachment, Guid> TepDinhKem =
        ServiceProvider.GetRequiredService<IRepository<Attachment, Guid>>();

    private readonly IUserProvider User = ServiceProvider.GetRequiredService<IUserProvider>();

    public async Task<PaginatedList<DeXuatNhuCauKinhPhiNamDto>> Handle(DeXuatNhuCauKinhPhiNamQuery request,
        CancellationToken cancellationToken = default) {

        DateTimeOffset? tuNgayDto = null;
        DateTimeOffset? denNgayExclusiveDto = null; 
        if (request.TuNgay.HasValue) {
            var dt = request.TuNgay.Value.ToDateTime(TimeOnly.MinValue);
            tuNgayDto = new DateTimeOffset(dt);
        }
        if (request.DenNgay.HasValue) {
            var dt = request.DenNgay.Value.ToDateTime(TimeOnly.MinValue);
            denNgayExclusiveDto = new DateTimeOffset(dt).AddDays(1);
        }

        var queryable = DeXuatNhuCauKinhPhiNam.GetQueryableSet().AsNoTracking()
          //  .WhereIf(request. != null, e => e.DuAnId == request.DuAnId)
            .WhereIf(request.So != null, e => e.So!.Contains(request.So!))
            .WhereIf(request.TrichYeu != null, e => e.So!.Contains(request.TrichYeu!))
            .WhereIf(request.TrangThaiId != null, e => e.TrangThaiId == request.TrangThaiId)
            .WhereIf(tuNgayDto != null, e => e.NgayKeHoach >= tuNgayDto)
            .WhereIf(denNgayExclusiveDto != null, e => e.NgayKeHoach < denNgayExclusiveDto);
        return await queryable
            .Select(e => new DeXuatNhuCauKinhPhiNamDto() {
                Id = e.Id,
             
                So = e.So,
                NgayKeHoach = e.NgayKeHoach,
                TrichYeu = e.TrichYeu,
                TongKinhPhiDeXuat = e.TongKinhPhiDeXuat,
                // trả thêm tên dự án
                TrangThaiId = e.TrangThaiId,
                MaTrangThai = e.TrangThai != null && e.TrangThai!.Ma != "LEG" ? e.TrangThai!.Ma : string.Empty,
                TenTrangThai = e.TrangThai != null && e.TrangThai!.Ma != "LEG" ? e.TrangThai!.Ten : string.Empty,
                DanhSachTepDinhKem = TepDinhKem.GetQueryableSet()
                    .Where(i => i.GroupId == e.Id.ToString())
                    .Select(i => i.ToDto()).ToList(),
            })
            .PaginatedListAsync(request.Skip(), request.Take(), cancellationToken: cancellationToken);
    }
}