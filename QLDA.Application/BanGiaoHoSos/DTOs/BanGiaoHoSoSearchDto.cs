namespace QLDA.Application.BanGiaoHoSos.DTOs;

// Chỉ 1 param từ UI (UserId luôn lấy từ Auth, không cho UI truyền)
public class BanGiaoHoSoSearchDto {
    public int? TrangThai { get; set; }  // 0: Khởi tạo, 1: Đã bàn giao
}
