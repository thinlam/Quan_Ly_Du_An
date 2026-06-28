using QLDA.Application.Common.Interfaces;
using QLDA.Application.TepDinhKems.DTOs;

namespace QLDA.Application.QuyetDinhLapBanQLDAs.DTOs;

public class QuyetDinhLapBanQldaDto : IHasKey<Guid?>,  IMayHaveTepDinhKemDto {
    [DefaultValue(null)] public Guid? Id { get; set; }
  
    public Guid DuAnId { get; set; }
    public int? BuocId { get; set; }
    public string? SoQuyetDinh { get; set; }
    public DateTimeOffset? NgayQuyetDinh { get; set; }
    public string? TrichYeu { get; set; }
    public string? NguoiKy { get; set; }
    public DateTimeOffset? NgayKy { get; set; }
    
    public string? SoDuThao { get; set; }
    public string? TrichYeuDuThao { get; set; }
    public string? TenTrangThai { get; set; }
    public string? MaTrangThai { get; set; }
    public int? TrangThaiId { get; set; }
   
    public List<TepDinhKemDto>? DanhSachTepDinhKem { get; set; }
}