using Microsoft.EntityFrameworkCore;
using QLDA.Application.Common.Interfaces;
using QLDA.Application.Common.Mapping;
using QLDA.Application.DeXuatNhuCauKinhPhis.DTOs;
using QLDA.Application.TepDinhKems.DTOs;
using QLDA.Domain.Constants;
using static Azure.Core.HttpHeader;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace QLDA.Application.DeXuatNhuCauKinhPhis.Queries;

public record DeXuatNhuCauKinhPhiComboboxQuery : AggregateRootPagination, IMayHaveGlobalFilter, IFromDateToDate, IRequest<PaginatedList<DeXuatNhuCauKinhPhiDto>> {
    public int? BuocId { get; set; }
    public Guid? DuAnId { get; set; }
    public bool IsNoTracking { get; set; }
    public string? GlobalFilter { get; set; }
    public int? TrangThaiId { get; set; }
    public string? SoPhieuChuyen { get; set; }
    public long? DonViDeXuatId { get; set; }
    public DateOnly? TuNgay { get; set; } 
    public DateOnly? DenNgay { get; set; }
}

internal class
    DeXuatNhuCauKinhPhiComboboxQueryHandler(IServiceProvider ServiceProvider)
    : IRequestHandler<DeXuatNhuCauKinhPhiComboboxQuery, PaginatedList<DeXuatNhuCauKinhPhiDto>> {
    private readonly IRepository<DeXuatNhuCauKinhPhi, Guid> DeXuatNhuCauKinhPhi = ServiceProvider.GetRequiredService<IRepository<DeXuatNhuCauKinhPhi, Guid>>();
    private readonly IRepository<TepDinhKem, Guid> TepDinhKem =
        ServiceProvider.GetRequiredService<IRepository<TepDinhKem, Guid>>();
    private readonly IRepository<DanhMucTrangThaiPheDuyet, int> _statusRepository =
      ServiceProvider.GetRequiredService<IRepository<DanhMucTrangThaiPheDuyet, int>>();


    private readonly IUserProvider User = ServiceProvider.GetRequiredService<IUserProvider>();

    public async Task<PaginatedList<DeXuatNhuCauKinhPhiDto>> Handle(DeXuatNhuCauKinhPhiComboboxQuery request,
        CancellationToken cancellationToken = default) {
        
        var trangThaiDaDuyetDx = await _statusRepository.GetQueryableSet(OnlyUsed: true, OnlyNotDeleted: true, OrderByIndex: false)
         .FirstOrDefaultAsync(s => s.Ma == TrangThaiPheDuyetCodes.DeXuatMacDinh.DaDuyet && s.Loai == PheDuyetEntityNames.DeXuatMacDinhStt, cancellationToken);
        var trangThaiTuChoiKH = await _statusRepository.GetQueryableSet(OnlyUsed: true, OnlyNotDeleted: true, OrderByIndex: false)
         .FirstOrDefaultAsync(s => s.Ma == TrangThaiPheDuyetCodes.DeXuatNhuCauKinhPhiNam.TuChoi && s.Loai == PheDuyetEntityNames.DeXuatNhuCauKinhPhiNam, cancellationToken);
       
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
        /* Combo Đề xuất của Kế hoạch năm hiển thị các đề xuất:
             Trạng thái đề xuất = Đã duyệt.
             Không thuộc Kế hoạch năm có trạng thái:
             Chờ duyệt
             Trả lại
             Đã duyệt.
             Nếu thuộc Kế hoạch năm có trạng thái Từ chối thì vẫn được phép hiển thị để chọn lại.
        */
        var queryable = DeXuatNhuCauKinhPhi.GetQueryableSet().AsNoTracking()
            .Where(e => !e.IsDeleted)
            .Where(e => !e.DuAn!.IsDeleted)
            .Where(    e => e.TrangThaiId == trangThaiDaDuyetDx.Id  )
            .WhereIf(request.DuAnId != null, e => e.DuAnId == request.DuAnId)
            .WhereIf(request.BuocId > 0, e => e.BuocId == request.BuocId)
            .WhereIf(request.SoPhieuChuyen != null, e => e.SoPhieuChuyen.Contains(request.SoPhieuChuyen))
            .WhereIf(tuNgayDto != null, e => e.NgayPhieuChuyen >= tuNgayDto)
            .WhereIf(denNgayExclusiveDto != null, e => e.NgayPhieuChuyen < denNgayExclusiveDto)
            // --- THÊM BỘ LỌC MỚI CỦA BẠN VÀO ĐÂY ---
            .Where(e => !e.DeXuatDaTrinhKeHoachNam!.Any() // TH1: Chưa có trong kế hoạch năm nào
                   || e.DeXuatDaTrinhKeHoachNam!.All(x =>
                        x.DeXuatNhuCauKinhPhiNam == null 
                        || x.DeXuatNhuCauKinhPhiNam.IsDeleted // TH2: Kế hoạch năm đã bị xóa
                        || x.DeXuatNhuCauKinhPhiNam.TrangThaiId == trangThaiTuChoiKH!.Id // TH3: Kế hoạch năm bị Từ chối
              )); 
        return await queryable
            .Select(e => new DeXuatNhuCauKinhPhiDto() {
                Id = e.Id,
                DuAnId = e.DuAnId,
                DonViDeXuatId = e.DonViDeXuatId,
                SoPhieuChuyen = e.SoPhieuChuyen,
                NgayPhieuChuyen = e.NgayPhieuChuyen,
                KinhPhiDeXuat = e.KinhPhiDeXuat,
                TenTrangThai = e.TrangThai != null && e.TrangThai.Ma != "LEG" ? e.TrangThai.Ten : TrangThaiPheDuyetCodes.Default.TenDuThao,
              
            })
            .PaginatedListAsync(request.Skip(), request.Take(), cancellationToken: cancellationToken);
    }
}