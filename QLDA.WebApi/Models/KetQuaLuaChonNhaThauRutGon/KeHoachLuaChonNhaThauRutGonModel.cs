using System.ComponentModel;
using QLDA.Domain.Interfaces;
using QLDA.WebApi.Models.TepDinhKems;
using SequentialGuid;

namespace QLDA.WebApi.Models.KeHoachLuaChonNhaThauRutGons;

public class KeHoachLuaChonNhaThauRutGonModel : IHasKey<Guid?>,  IMayHaveTepDinhKemModel{
    [DefaultValue(null)] public Guid? Id { get; set; }
    public Guid GetId()
    {
        Id ??= SequentialGuidGenerator.Instance.NewGuid();
        return (Guid)Id;
    }
    
    public Guid DuAnId { get; set; }
    public int? BuocId { get; set; }
    public Guid? NhaThauId { get; set; }
    public Guid? GoiThauId { get; set; }
    public int? KetQuaDanhGia { get; set; }
    public List<TepDinhKemModel>? DanhSachTepDinhKem { get; set; }
}