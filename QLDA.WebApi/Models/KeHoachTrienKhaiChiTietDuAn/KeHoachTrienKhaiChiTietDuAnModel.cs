using System.ComponentModel;
using QLDA.Domain.Interfaces;
using QLDA.WebApi.Models.TepDinhKems;
using SequentialGuid;

namespace QLDA.WebApi.Models.CanBoTrienKhaiHangMucs;


public class KeHoachTrienKhaiChiTietDuAnModel : IHasKey<Guid?>, IMustHaveId<Guid>, IMayHaveTepDinhKemModel, ITienDo
{
    [DefaultValue(null)]
    public Guid? Id { get; set; }
    public Guid GetId()
    {
        Id ??= SequentialGuidGenerator.Instance.NewGuid();
        return (Guid)Id;
    }

    public Guid SetId()
    {

        return SequentialGuidGenerator.Instance.NewGuid();
    }
    public string? Ten { get; set; }
    public string? MaMoc { get; set; }
    public Guid DuAnId { get; set; }
    public int? BuocId { get; set; }
    public int? TiLeHoanThanh { get; set; }
    public int? TrangThaiId { get; set; }
    public int? DonViChuTriId { get; set; }
    public string? GhiChu { get; set; }
    public DateOnly? NgayBatDauKeHoach { get; set; }
    public DateOnly? NgayKetThucKeHoach { get; set; }
    public DateOnly? NgayKetThucThucTe { get; set; }
    public DateOnly? NgayBatDauThucTe { get; set; }
    public List<TepDinhKemModel>? DanhSachTepDinhKem { get; set; }
}