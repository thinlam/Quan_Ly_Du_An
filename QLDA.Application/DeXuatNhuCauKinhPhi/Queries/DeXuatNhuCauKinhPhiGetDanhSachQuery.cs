using Microsoft.EntityFrameworkCore;
using QLDA.Application.Common.Interfaces;
using QLDA.Application.Common.Mapping;
using QLDA.Application.TepDinhKems.DTOs;
using QLDA.Application.DeXuatNhuCauKinhPhis.DTOs;
using QLDA.Domain.Constants;

namespace QLDA.Application.DeXuatNhuCauKinhPhis.Queries;

public record DeXuatNhuCauKinhPhiQuery : AggregateRootPagination, IMayHaveGlobalFilter, IFromDateToDate, IRequest<PaginatedList<DeXuatNhuCauKinhPhiDto>> {
    public int? BuocId { get; set; }
    public Guid? DuAnId { get; set; }
    public bool IsNoTracking { get; set; }
    public string? GlobalFilter { get; set; }
    public int? TrangThaiId { get; set; }
    public bool? DaDuyetTongHop { get; set; }
    public string? SoPhieuChuyen { get; set; }
    public long? DonViDeXuatId { get; set; }
    public DateOnly? TuNgay { get; set; } 
    public DateOnly? DenNgay { get; set; }
}

internal class
    DeXuatNhuCauKinhPhiQueryHandler(IServiceProvider ServiceProvider)
    : IRequestHandler<DeXuatNhuCauKinhPhiQuery, PaginatedList<DeXuatNhuCauKinhPhiDto>> {
    private readonly IRepository<DeXuatNhuCauKinhPhi, Guid> DeXuatNhuCauKinhPhi = ServiceProvider.GetRequiredService<IRepository<DeXuatNhuCauKinhPhi, Guid>>();
    private readonly IRepository<TepDinhKem, Guid> TepDinhKem =
        ServiceProvider.GetRequiredService<IRepository<TepDinhKem, Guid>>();
    private readonly IRepository<DanhMucTrangThaiPheDuyet, int> _statusRepository =
      ServiceProvider.GetRequiredService<IRepository<DanhMucTrangThaiPheDuyet, int>>();


    private readonly IUserProvider User = ServiceProvider.GetRequiredService<IUserProvider>();

    public async Task<PaginatedList<DeXuatNhuCauKinhPhiDto>> Handle(DeXuatNhuCauKinhPhiQuery request,
        CancellationToken cancellationToken = default) {
        
        var trangThaiDaDuyet = await _statusRepository.GetQueryableSet(OnlyUsed: true, OnlyNotDeleted: true, OrderByIndex: false)
         .FirstOrDefaultAsync(s => s.Ma == TrangThaiPheDuyetCodes.DeXuatMacDinh.DaDuyet && s.Loai == PheDuyetEntityNames.DeXuatMacDinhStt, cancellationToken);
       
        ManagedException.ThrowIfNull(trangThaiDaDuyet, "Không tìm thấy trạng thái 'Đã duyệt'"); 

        DateTimeOffset? tuNgayDto = null;
        DateTimeOffset? denNgayExclusiveDto = null; // exclusive upper bound
        if (request.TuNgay.HasValue) {
            var dt = request.TuNgay.Value.ToDateTime(TimeOnly.MinValue);
            tuNgayDto = new DateTimeOffset(dt);
        }
        if (request.DenNgay.HasValue) {
            var dt = request.DenNgay.Value.ToDateTime(TimeOnly.MinValue);
            denNgayExclusiveDto = new DateTimeOffset(dt).AddDays(1);
        }
        //Phục vụ.
        
        var queryable = DeXuatNhuCauKinhPhi.GetQueryableSet().AsNoTracking()
            .Where(e => !e.DuAn!.IsDeleted)

            .WhereIf(
                request.DaDuyetTongHop ?? false, e => e.TrangThaiId == trangThaiDaDuyet.Id
            )
            .WhereIf(request.DuAnId != null, e => e.DuAnId == request.DuAnId)
            .WhereIf(request.BuocId > 0, e => e.BuocId == request.BuocId)
            .WhereIf(request.SoPhieuChuyen != null, e => e.SoPhieuChuyen.Contains(request.SoPhieuChuyen))
            .WhereIf(request.TrangThaiId != null, e => e.TrangThaiId == request.TrangThaiId )
            .WhereIf(tuNgayDto != null, e => e.NgayPhieuChuyen >= tuNgayDto)
            .WhereIf(denNgayExclusiveDto != null, e => e.NgayPhieuChuyen < denNgayExclusiveDto);
        return await queryable
            .Select(e => new DeXuatNhuCauKinhPhiDto() {
                Id = e.Id,
                DuAnId = e.DuAnId,
                BuocId = e.BuocId,

                DonViDeXuatId = e.DonViDeXuatId,
                SoPhieuChuyen = e.SoPhieuChuyen,
                NgayPhieuChuyen = e.NgayPhieuChuyen,
                TrichYeu = e.TrichYeu,
                KinhPhiDeXuat = e.KinhPhiDeXuat,
                TrangThaiId = e.TrangThaiId,

                MaTrangThai = e.TrangThai != null && e.TrangThai.Ma != "LEG" ? e.TrangThai.Ma : TrangThaiPheDuyetCodes.Default.DuThao,
                TenTrangThai = e.TrangThai != null && e.TrangThai.Ma != "LEG" ? e.TrangThai.Ten : TrangThaiPheDuyetCodes.Default.TenDuThao,
                DanhSachTepDinhKem = TepDinhKem.GetQueryableSet()
                    .Where(i => i.GroupId == e.Id.ToString())
                    .Select(i => i.ToDto()).ToList(),
            })
            .PaginatedListAsync(request.Skip(), request.Take(), cancellationToken: cancellationToken);
    }
}