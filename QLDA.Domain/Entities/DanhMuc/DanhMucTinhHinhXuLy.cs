namespace QLDA.Domain.Entities.DanhMuc;

/// <summary>
/// Danh mục chức vụ
/// </summary>
public class DanhMucTinhHinhXuLy : DanhMuc<int>, IAggregateRoot , IMayHaveStt {
    public int? Stt { get; set; } = null!;
}