using QLDA.Application.Common.Interfaces;
using QLDA.Application.TepDinhKems.DTOs;
using QLDA.Domain.Interfaces;
using SequentialGuid;

namespace QLDA.Application.DuToanDauTus.DTOs;

public class DuToanDauTuDto : IHasKey<Guid?>, IMustHaveId<Guid>, ITienDo, IMayHaveTepDinhKemDto {
    [DefaultValue(null)] public Guid? Id { get; set; }

    public Guid GetId() {
        Id ??= SequentialGuidGenerator.Instance.NewGuid();
        return (Guid)Id;
    }

    public Guid DuAnId { get; set; }
    public int? BuocId { get; set; }
    public DateTimeOffset? NgayTrinh { get; set; }
    public string SoToTrinh { get; set; } = string.Empty;
    public string? TrichYeu { get; set; }


    public int? PhuongAnThietKeId { get; set; }
    public string? TenPhuongAnThietKe { get; set; }
    public long? TongMucDauTu { get; set; }
    public long? TongDuToan { get; set; }
    //public string? NoiDungChiPhis { get; set; }
    public int? NguonVonId { get; set; }
    public string? TenNguonVon { get; set; }
    public string? NoiDungChiPhi { get; set; }
    public int? Nam { get; set; }
    

    public int? TrangThaiId { get; set; }
    public string? MaTrangThai { get; set; }
    public string? TenTrangThai { get; set; }
    public List<TepDinhKemDto>? DanhSachTepDinhKem { get; set; }
}