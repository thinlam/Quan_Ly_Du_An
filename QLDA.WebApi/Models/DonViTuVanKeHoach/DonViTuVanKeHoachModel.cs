using QLDA.WebApi.Models.TepDinhKems;
using SequentialGuid;
using System.ComponentModel;

namespace QLDA.WebApi.Models.DonViTuVanKeHoachs;

public class DonViTuVanKeHoachModel : IHasKey<Guid?>, IMayHaveTepDinhKemModel
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


    public string TenDonVi { get; set; }
    public Guid KeHoachId { get; set; }
    public List<TepDinhKemModel>? DanhSachTepDinhKem { get; set; }

}