namespace QLDA.Application.BanGiaoHoSos.DTOs;

// CreatedBy luôn lấy từ Auth, không cho UI truyền
public class BanGiaoHoSoSearchDto {
    public int? TrangThai { get; set; }  // 0: Khởi tạo, 1: Đã bàn giao
    public Guid? DuAnId { get; set; }   // Lọc theo dự án
    public int? BuocId { get; set; }    // Lọc theo bước
}
