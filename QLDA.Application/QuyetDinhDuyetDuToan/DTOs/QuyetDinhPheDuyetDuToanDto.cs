using QLDA.Application.Common.Interfaces;
using QLDA.Application.QuyetDinhDuyetDuToans.DTOs;
using QLDA.Application.TepDinhKems.DTOs;

namespace QLDA.Application.QuyetDinhDuyetDuToanDtos.DTOs;

public class QuyetDinhDuyetDuToanDto : IHasKey<Guid?>,  IMayHaveTepDinhKemDto {
    [DefaultValue(null)] public Guid? Id { get; set; }
  
    public Guid DuAnId { get; set; }
    public int? BuocId { get; set; }
    public int? TrangThaiId { get; set; }

    public string? SoQuyetDinh { get; set; }
    public DateTimeOffset? NgayQuyetDinh { get; set; }
    public string? TrichYeu { get; set; }

    public string? TenDuAn {  get; set; }
    public string? TenTrangThai {  get; set; }
    public string? TenHinhThucQuanLy {  get; set; }
    public string? TenKeHoachLuaChonNhaThau {  get; set;}
    public decimal? GiaTri {  get; set;}
    public string ThoiGian { get; set; } = string.Empty;
    public List<QuyetDinhDuyetDuToanNguonVonDto>? KeHoachVons { get; set; }
    public List<QuyetDinhDuyetDuToanChiPhiDto>? ChiPhis { get; set; }
    public List<TepDinhKemDto>? DanhSachTepDinhKem { get; set; }
}