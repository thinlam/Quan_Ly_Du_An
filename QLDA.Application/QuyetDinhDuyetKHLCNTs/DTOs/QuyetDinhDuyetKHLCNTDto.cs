using QLDA.Application.Common.Interfaces;
using QLDA.Application.TepDinhKems.DTOs;
using QLDA.Application.TongHopVanBanQuyetDinhs.DTOs;
using SequentialGuid;

namespace QLDA.Application.QuyetDinhDuyetKHLCNTs.DTOs;

public class QuyetDinhDuyetKHLCNTDto : IHasKey<Guid?>, IMustHaveId<Guid>, IMayHaveTepDinhKemDto{//, ITienDo
    [DefaultValue(null)] public Guid? Id { get; set; }

    public Guid GetId() {
        Id ??= SequentialGuidGenerator.Instance.NewGuid();
        return (Guid)Id;
    }

  
    public Guid? KeHoachLuaChonNhaThauId { get; set; }

    /// <summary>
    /// Số quyết định
    /// </summary>
//public string? SoQuyetDinh { get; set; }
    public VanBanQuyetDinhDto? VanBanQuyetDinh { get; set; }
    public List<TepDinhKemDto>? DanhSachTepDinhKem { get; set; }
}