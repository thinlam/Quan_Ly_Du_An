namespace QLDA.Domain.Entities.DanhMuc;
/// <summary>
/// Danh mục trạng thái dự án
/// </summary>
public class DanhMucHinhThucLuaChonNhaThau : DanhMuc<int>, IAggregateRoot, IMayHaveStt {
    public int? Stt { get; set; }
    public bool? LaChiDinhThau { get; set; }

    #region Navigation Properties

    public ICollection<GoiThau>? GoiThaus { get; set; } = [];
    
    #endregion
}