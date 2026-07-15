namespace QLDA.Application.BanGiaoHoSos.DTOs;

// CreatedBy luôn lấy từ Auth, không cho UI truyền
public class BanGiaoHoSoSearchDto : IMayHaveGlobalFilter {
    public int? TrangThai { get; set; }  // 1: Khởi tạo, 2: Đã bàn giao
    public Guid? DuAnId { get; set; }   // Lọc theo dự án
    public int? BuocId { get; set; }    // Lọc theo bước
    public string? GlobalFilter { get; set; }
}
