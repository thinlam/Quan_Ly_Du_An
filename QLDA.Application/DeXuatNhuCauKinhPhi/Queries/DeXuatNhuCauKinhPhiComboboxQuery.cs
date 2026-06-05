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
    public Guid? KeHoachId { get; set; }
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
        /*
         3 case 
        1 thuộc kehoachNam.id = request.KeHoachId
        2 không thuộc kehoachNam nào
        3 thuộc kehoachNam nhưng keHoachNam.TrangThai = Từ chối || keHoachNam.IsDeleted = true
        */
        // Case 1: Thuộc chính kế hoạch năm đang truyền lên trong request (Nếu có truyền KeHoachId)
        .Where(e =>
        // Nếu Đề xuất thuộc chính kế hoạch này thì luôn được lấy (để giữ liên kết cũ khi sửa)
        (request.KeHoachId.HasValue &&
        e.DeXuatDaTrinhKeHoachNam!.Any(x => x.DeXuatNhuCauKinhPhiNam != null
        && x.DeXuatNhuCauKinhPhiNam.Id == request.KeHoachId.Value))

        // HOẶC: Đề xuất này KHÔNG ĐƯỢC TỒN TẠI trong một Kế hoạch năm nào khác đang "Hợp lệ" (Chưa xóa và Chưa bị từ chối)
        || !e.DeXuatDaTrinhKeHoachNam!.Any(x =>
            x.DeXuatNhuCauKinhPhiNam != null
            && x.DeXuatNhuCauKinhPhiNam.IsDeleted                                      // Kế hoạch đang hoạt động
            && x.DeXuatNhuCauKinhPhiNam.TrangThaiId == trangThaiTuChoiKH!.Id             // Trạng thái KHÁC Từ chối (ví dụ: Chờ duyệt, Đã duyệt)
            
            )
        );
      
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