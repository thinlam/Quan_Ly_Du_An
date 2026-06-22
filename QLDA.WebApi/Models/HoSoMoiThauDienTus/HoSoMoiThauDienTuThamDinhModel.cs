using SequentialGuid;
using QLDA.WebApi.Models.TepDinhKems;

namespace QLDA.WebApi.Models.HoSoMoiThauDienTus;


public class HoSoMoiThauThamDinhModel
{
    public Guid? NhaThauId { get; set; }

    public Guid GetId()
    {
        NhaThauId ??= SequentialGuidGenerator.Instance.NewGuid();
        return (Guid)NhaThauId;
    }
    
    public List<TepDinhKemModel>? DinhKemQuyetDinh { get; set; }
    public List<TepDinhKemModel>? DinhKemCamKet { get; set; }
    public List<TepDinhKemModel>? DinhKemBaoCao { get; set; }


}