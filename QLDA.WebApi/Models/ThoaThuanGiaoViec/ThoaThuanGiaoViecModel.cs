using System.ComponentModel;
using QLDA.WebApi.Models.TepDinhKems;
using SequentialGuid;

namespace QLDA.WebApi.Models.ThoaThuanGiaoViecs;

public class ThoaThuanGiaoViecModel : IHasKey<Guid?>,  IMayHaveTepDinhKemModel{
    [DefaultValue(null)] public Guid? Id { get; set; }
    public Guid GetId()
    {
        Id ??= SequentialGuidGenerator.Instance.NewGuid();
        return (Guid)Id;
    }
    
    public Guid DuAnId { get; set; }
    public int? BuocId { get; set; }
    public int? TrangThaiId { get; set; }
    public Guid? GoiThauId { get; set; }
    public string? PhamVi { get; set; }
    public int? ThoiGian { get; set; }
    public long? GiaTri { get; set; }
    public string? NoiDung { get; set; }
    public string? ChatLuong { get; set; }

    public List<TepDinhKemModel>? DanhSachTepDinhKem { get; set; }
}