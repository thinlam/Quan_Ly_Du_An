using QLDA.Application.Common.Interfaces;
using QLDA.Application.TepDinhKems.DTOs;
using QLDA.Domain.Interfaces;
using SequentialGuid;

namespace QLDA.Application.QuyetDinhDuyetDuToans.DTOs;

public class QuyetDinhDuyetDuToanInsUpdDto : IHasKey<Guid?>, IMustHaveId<Guid> ,IMayHaveTepDinhKemDto, ITienDo {
    [DefaultValue(null)] public Guid? Id { get; set; }

    public Guid GetId()
    {
        Id ??= SequentialGuidGenerator.Instance.NewGuid();
        return (Guid)Id;
    }

    public Guid DuAnId { get; set; }
    public int? BuocId { get; set; }
    public string? So { get; set; }
    public DateTimeOffset? Ngay { get; set; }
    public string? TrichYeu { get; set; }

    public decimal? GiaTri { get; set; }
    public int? HinhThucQuanLyId { get; set; }
    public string? ThoiGianThucHien { get; set; }
    public Guid? KeHoachLuaChonNhaThauId { get; set; }

    public List<QuyetDinhDuyetDuToanNguonVonDto>? KeHoachVons { get; set; } = [];
    public List<QuyetDinhDuyetDuToanChiPhiDto>? ChiPhis { get; set; } = [];
    public List<TepDinhKemDto>? DanhSachTepDinhKem { get; set; }
    public List<TepDinhKemDto>? DanhSachTepDinhKemKhac { get; set; }
    
}