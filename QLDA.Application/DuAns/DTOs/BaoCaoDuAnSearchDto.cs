using QLDA.Application.Common.Interfaces;

namespace QLDA.Application.DuAns.DTOs;

public record BaoCaoDuAnSearchDto : CommonSearchDto {
    ///<summary>
    /// Tên dự án       
    ///</summary>
    public string? TenDuAn { get; set; }
    
    ///<summary>
    /// Thời gian khởi công
    ///</summary>
    public int? ThoiGianKhoiCong { get; set; }
    
    ///<summary>
    /// Thời gian hoàn thành
    ///</summary>
    public int? ThoiGianHoanThanh { get; set; }
    
    ///<summary>
    /// Loại dự án theo năm (tính chất vốn)
    ///</summary>
    public int? LoaiDuAnTheoNamId { get; set; }
    
    ///<summary>
    /// Hình thức đầu tư
    ///</summary>
    public int? HinhThucDauTuId { get; set; }
    
    ///<summary>
    /// Loại dự án
    ///</summary>
    public int? LoaiDuAnId { get; set; }
    
    ///<summary>
    /// Đơn vị phụ trách chính (Phòng ban phụ trách)
    ///</summary>
    public long? DonViPhuTrachChinhId { get; set; }
}
