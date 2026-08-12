namespace QLDA.Domain.Enums;

/// <summary>
/// Phân biệt loại bước xử lý trong bảng dùng chung <see cref="Entities.ToTrinhThamDinhBuocXuLy"/>
/// (Đối chiếu / Thương thảo / Thẩm định) — Issue #179.
/// </summary>
public enum ELoaiBuocXuLyThamDinhNhaThau
{
    DoiChieu = 1,
    ThuongThao = 2,
    ThamDinh = 3,
}
