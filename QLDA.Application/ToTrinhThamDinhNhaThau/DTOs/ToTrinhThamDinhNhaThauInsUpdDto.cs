using QLDA.Application.Common.Interfaces;
using QLDA.Application.TepDinhKems.DTOs;
using QLDA.Domain.Interfaces;

namespace QLDA.Application.ToTrinhThamDinhNhaThaus.DTOs;

public class ToTrinhThamDinhNhaThauInsUpdDto : IMayHaveTepDinhKemDto
{
    [DefaultValue(null)] public Guid? Id { get; set; }
  
    public string? So { get; set; }
    public Guid DuAnId { get; set; }
    public int? BuocId { get; set; }
    public DateTimeOffset NgayTrinh{ get; set; }
    public string? TrichYeu { get; set; }
    public int? TrangThaiId { get; set; }
    public int? TrangThaiDangTaiId { get; set; }
    public int? TrangThaiThamDinhId { get; set; }
    public List<TepDinhKemDto>? DanhSachTepDinhKem { get; set; }
    public List<TepDinhKemDto>? DanhSachTepThamDinh { get; set; }
    public List<KetQuaThamDinhNhaThau>? DanhSachNhaThau { get; set; }


}