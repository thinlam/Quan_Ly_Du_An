using System.ComponentModel;
using QLDA.Domain.Interfaces;
using QLDA.WebApi.Models.TepDinhKems;
using SequentialGuid;

namespace QLDA.WebApi.Models.KetQuaThamDinhNhaThaus;

public class KetQuaThamDinhNhaThauModel : IHasKey<Guid?>,  IMayHaveTepDinhKemModel{
    [DefaultValue(null)] public Guid? Id { get; set; }
    public Guid GetId()
    {
        Id ??= SequentialGuidGenerator.Instance.NewGuid();
        return (Guid)Id;
    }

    public Guid SetId()
    {

        return SequentialGuidGenerator.Instance.NewGuid();
    }
    
    public Guid ToTrinhId { get; set; }
    public Guid NhaThauId { get; set; }
    public Guid GoiThauId { get; set; }
    public string? KetQuaDanhGia { get; set; }
    public List<TepDinhKemModel>? DanhSachTepDinhKem { get; set; }
}