using QLDA.Application.Common.Interfaces;
using QLDA.Application.TepDinhKems.DTOs;
using QLDA.Domain.Interfaces;
using SequentialGuid;

namespace QLDA.Application.KeHoachLuaChonNhaThauRutGons.DTOs;

public class KeHoachLuaChonNhaThauRutGonDto : IHasKey<Guid?>, IMustHaveId<Guid>, ITienDo, IMayHaveTepDinhKemDto {
    [DefaultValue(null)] public Guid? Id { get; set; }

    public Guid GetId() {
        Id ??= SequentialGuidGenerator.Instance.NewGuid();
        return (Guid)Id;
    }

    public Guid DuAnId { get; set; }
    public string? TenDuAn { get; set; }
    public int? BuocId { get; set; }
    public Guid? GoiThauId { get; set; }
    public string? TenGoiThau { get; set; }
    public Guid? NhaThauId { get; set; }// đơn vị tư vấn
    public string? TenNhaThau { get; set; }
    public int? KetQuaDanhGia { get; set; }
    
    public int? TrangThaiId { get; set; }
    public string? MaTrangThai { get; set; }
    public string? TenTrangThai { get; set; }
    public List<TepDinhKemDto>? DanhSachTepDinhKem { get; set; }
}
