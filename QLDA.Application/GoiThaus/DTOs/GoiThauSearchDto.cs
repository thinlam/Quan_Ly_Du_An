using QLDA.Application.Common.Interfaces;

namespace QLDA.Application.GoiThaus.DTOs;

public record GoiThauSearchDto : CommonSearchDto {
    public string? Ten { get; set; }
    public Guid? HopDongId { get; set; }
    public Guid? KetQuaTrungThauId { get; set; }
    public int? NguonVonId { get; set; }
    public int? LoaiHopDongId { get; set; }
    public int? LoaiGoiThauId { get; set; }
    public int? PhuongThucLuaChonNhaThauId { get; set; }
    public Guid? KeHoachLuaChonNhaThauId { get; set; }
    public int? HinhThucLuaChonNhaThauId { get; set; }
    /// <summary>
    /// Loại dự án theo năm - tài chính
    /// </summary>
    /// <remarks>PMIS #9609</remarks>
    public int? LoaiDuAnTheoNamId { get; set; }

    /// <summary>
    /// Khi true: chỉ lấy gói thầu đã tích thẩm định bên E-HSMT (HoSoMoiThauDienTu.ThamDinh).
    /// Không truyền / null: giữ behavior cũ.
    /// </summary>
    /// <remarks>Issue #169 — màn 9667</remarks>
    public bool? IsThamDinh { get; set; }
}