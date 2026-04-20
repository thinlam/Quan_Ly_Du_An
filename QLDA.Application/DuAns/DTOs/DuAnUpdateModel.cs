using QLDA.Application.DuToans.DTOs;
using QLDA.Domain.Interfaces;

namespace QLDA.Application.DuAns.DTOs;

public class DuAnUpdateModel : IHasKey<Guid>, IMayHaveParent<Guid?> {
    public Guid Id { get; set; }
    public Guid? ParentId { get; set; }
    public string? TenDuAn { get; set; }
    public int? QuyTrinhId { get; set; }
    public string? DiaDiem { get; set; }
    public int? ChuDauTuId { get; set; }
    public int? ThoiGianKhoiCong { get; set; }
    public int? ThoiGianHoanThanh { get; set; }
    public string? MaDuAn { get; set; }
    public string? MaNganSach { get; set; }
    public bool DuAnTrongDiem { get; set; } = false;
    public int? LinhVucId { get; set; }
    public int? NhomDuAnId { get; set; }
    public string? NangLucThietKe { get; set; }
    public string? QuyMoDuAn { get; set; }
    public int? HinhThucQuanLyDuAnId { get; set; }
    public int? HinhThucDauTuId { get; set; }
    public int? LoaiDuAnId { get; set; }
    public long? TongMucDauTu { get; set; }
    public List<int>? DanhSachNguonVon { get; set; } = [];
    public int? LoaiDuAnTheoNamId { get; set; }
    public int? TrangThaiDuAnId { get; set; }
    public string? GhiChu { get; set; }
    public DateTimeOffset? NgayBatDau { get; set; }
    public long? LanhDaoPhuTrachId { get; set; } = null;
    public long? DonViPhuTrachChinhId { get; set; }
    public List<long>? DonViPhoiHopIds { get; set; }
    public List<DuToanUpdateModel>? DuToans { get; set; }
    public long? DuToanBanDauId { get; set; }
    public long? SoDuToanBanDau { get; set; }
    /// <summary>
    /// Khái toán kinh phí
    /// </summary>
    public decimal? KhaiToanKinhPhi { get; set; }
}