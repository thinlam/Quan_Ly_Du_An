using QLDA.Application.Common.Interfaces;
using QLDA.Application.TepDinhKems.DTOs;
using QLDA.Domain.Interfaces;
using SequentialGuid;

namespace QLDA.Application.ThoaThuanGiaoViecs.DTOs;

public class ThoaThuanGiaoViecDto : IHasKey<Guid?>, IMustHaveId<Guid>, ITienDo, IMayHaveTepDinhKemDto {
    [DefaultValue(null)] public Guid? Id { get; set; }

    public Guid GetId() {
        Id ??= SequentialGuidGenerator.Instance.NewGuid();
        return (Guid)Id;
    }

    public Guid DuAnId { get; set; }
    public int? BuocId { get; set; }
    public Guid? GoiThauId { get; set; }
    public string? TenDuAn { get; set; }
    public string? TenGoiThau { get; set; }
    public string? PhamVi { get; set; }
    public int? ThoiGian { get; set; }
    public long? GiaTri { get; set; }
    public string? NoiDung { get; set; }
    public string? ChatLuong { get; set; }


    public int? TrangThaiId { get; set; }
    public string? MaTrangThai { get; set; }
    public string? TenTrangThai { get; set; }
    public List<TepDinhKemDto>? DanhSachTepDinhKem { get; set; }
}