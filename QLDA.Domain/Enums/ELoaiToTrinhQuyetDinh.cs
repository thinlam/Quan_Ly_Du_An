namespace QLDA.Domain.Enums;

/// <summary>
/// Phân biệt nghiệp vụ sở hữu 1 dòng <see cref="Entities.ToTrinhQuyetDinh"/> dùng chung
/// (xác định bằng cặp EntityId + Loai thay vì mỗi nghiệp vụ 1 FK riêng).
/// </summary>
public enum ELoaiToTrinhQuyetDinh
{
    HoSoMoiThauToTrinh = 1,
    HoSoMoiThauQuyetDinh = 2,
    ToTrinhThamDinhNhaThau = 3,
}
