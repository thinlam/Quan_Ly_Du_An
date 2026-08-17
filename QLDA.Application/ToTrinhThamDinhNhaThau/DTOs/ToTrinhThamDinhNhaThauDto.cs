using QLDA.Application.Common.Interfaces;
using QLDA.Application.TepDinhKems.DTOs;
using SequentialGuid;

namespace QLDA.Application.ToTrinhThamDinhNhaThaus.DTOs;

public class ToTrinhThamDinhNhaThauDto : IHasKey<Guid?>, IMustHaveId<Guid>, IMayHaveTepDinhKemDto {
    [DefaultValue(null)] public Guid? Id { get; set; }

    public Guid GetId() {
        Id ??= SequentialGuidGenerator.Instance.NewGuid();
        return (Guid)Id;
    }

    public Guid DuAnId { get; set; }
    public int? BuocId { get; set; }
    public Guid? GoiThauId { get; set; }
    public string TenGoiThau { get; set; } = string.Empty;
    public Guid? NhaThauId { get; set; }
    public int? TrangThaiId { get; set; }
    public string? MaTrangThai { get; set; }
    public int? TrangThaiDangTaiId { get; set; }
    public string? TenTrangThai { get; set; }
    public List<TepDinhKemDto>? DanhSachTepDinhKem { get; set; }
    public List<TepDinhKemDto>? DanhSachTepThamDinh { get; set; }

    /// <summary>3 bước xử lý (Issue #179) — expose trực tiếp, không bắt FE tự find/filter theo Loai.</summary>
    public ToTrinhThamDinhBuocXuLyDto? DoiChieu { get; set; }
    public ToTrinhThamDinhBuocXuLyDto? ThuongThao { get; set; }
    public ToTrinhThamDinhBuocXuLyDto? ThamDinh { get; set; }
}
//Id = model.GetId(),

public class KetQuaThamDinhNhaThauDto
{
    public Guid Id { get; set; }
    public Guid ToTrinhId { get; set; }
    public Guid NhaThauId { get; set; }
    public Guid GoiThauId { get; set; }
    public string? KetQuaDanhGia { get; set; }
    public List<TepDinhKemDto>? DanhSachTepDinhKem  { get; set; }
  

}

  