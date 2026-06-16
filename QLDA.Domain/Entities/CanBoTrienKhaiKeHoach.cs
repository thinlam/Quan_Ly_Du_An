using BuildingBlocks.Domain.Entities;
using QLDA.Domain.Entities.DanhMuc;

namespace QLDA.Domain.Entities;

/// <summary>
/// Bảng quản lý hồ sơ mời thầu điện tử
/// </summary>
public class CanBoTrienKhaiHangMuc {
    public Guid KeHoachId { get; set; }
    public long CanBoId { get; set; }
    public KeHoachTrienKhaiHangMuc KeHoachTrienKhai { get; set; } = default!;

}