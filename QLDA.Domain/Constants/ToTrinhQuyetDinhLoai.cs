namespace QLDA.Domain.Constants;

/// <summary>
/// Giá trị <see cref="Entities.ToTrinhQuyetDinh.Loai"/> — lưu string có ý nghĩa nghiệp vụ
/// (thay numeric enum) để đọc trực tiếp trên DB dễ hơn. Dùng chung bảng
/// <see cref="Entities.ToTrinhQuyetDinh"/> cho nhiều nghiệp vụ qua EntityId + Loai.
/// </summary>
public static class ToTrinhQuyetDinhLoai
{
    /// <summary>Tờ trình của HoSoMoiThauDienTu.</summary>
    public const string HoSoMoiThauToTrinh = "HoSoMoiThauToTrinh";
    /// <summary>Quyết định của HoSoMoiThauDienTu.</summary>
    public const string HoSoMoiThauQuyetDinh = "HoSoMoiThauQuyetDinh";
    /// <summary>Tờ trình kết quả của ToTrinhThamDinhNhaThau.</summary>
    public const string ToTrinhThamDinhNhaThau = "ToTrinhThamDinhNhaThau";
}
