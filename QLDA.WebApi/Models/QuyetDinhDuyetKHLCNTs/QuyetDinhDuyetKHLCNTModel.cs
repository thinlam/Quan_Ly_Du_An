using QLDA.WebApi.Models.TepDinhKems;
using QLDA.WebApi.Models.TongHopVanBanQuyetDinhs;
using SequentialGuid;
using System.ComponentModel;

namespace QLDA.WebApi.Models.QuyetDinhDuyetKHLCNTs;

public class QuyetDinhDuyetKHLCNTModel : IHasKey<Guid?>, IMustHaveId<Guid>, IMayHaveTepDinhKemModel { //}, ITienDo {
    [DefaultValue(null)] public Guid? Id { get; set; }

    public Guid GetId() {
        Id ??= SequentialGuidGenerator.Instance.NewGuid();
        return (Guid)Id;
    }
    
    public VanBanQuyetDinhModel? VanBanQuyetDinh { get; set; }
   
    public Guid? KeHoachLuaChonNhaThauId { get; set; }

    public long TongDuToan { get; set; }
    public long? DuToanThamDinh { get; set; }
    public int? NguonVonId { get; set; }
    public int? ThoiGianThucHien { get; set; }
    public int? SoLuongGoiThau { get; set; }

    public List<TepDinhKemModel>? DanhSachTepDinhKem { get; set; }
}