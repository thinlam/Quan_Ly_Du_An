using QLDA.Application.Common.Interfaces;
using QLDA.Application.TepDinhKems.DTOs;
using QLDA.Domain.Interfaces;
using SequentialGuid;

namespace QLDA.Application.DeXuatNhuCauKinhPhis.DTOs;

public class TheoDoiDeXuatNhuCauKinhPhiDto : IHasKey<Guid?>, IMustHaveId<Guid>, ITienDo {
    [DefaultValue(null)] public Guid? Id { get; set; }

    public Guid GetId() {
        Id ??= SequentialGuidGenerator.Instance.NewGuid();
        return (Guid)Id;
    }

    public Guid DuAnId { get; set; }
    public int? BuocId { get; set; }
    public long? DonViDeXuatId { get; set; }
    public string? TenDonViDeXuat { get; set; }
    public string? SoPhieuChuyen { get; set; }
    public DateTimeOffset? NgayPhieuChuyen { get; set; }
    public string? TrichYeu { get; set; }
    public long? KinhPhiDeXuat { get; set; }
  
    public string? SoKeHoach { get; set; }// phía user đã trình
    public string? NgayKeHoach { get; set; }// phía user đã trình
  
    public int? TrangThaiId { get; set; } // phía user đã trình
    public string? TenTrangThai { get; set; }// phía user đã trình

    public int? TrangThaiKeHoachNamId { get; set; } // phía phòng TCKH đã duyệt/trả kế hoạch
    public string? TenTrangThaiKeHoachNam { get; set; } // if()đã trình / ---
    public string? TenTrangThaiBanGiamDoc { get; set; } // phía ban GD đã duyệt/trả

    public DateTimeOffset? NgayDuyetDeXuat { get; set; }
    public DateTimeOffset? NgayDuyetKeHoach { get; set; }
}
