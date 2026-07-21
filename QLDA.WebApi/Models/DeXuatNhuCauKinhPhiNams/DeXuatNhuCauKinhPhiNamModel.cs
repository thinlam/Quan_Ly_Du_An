using System.ComponentModel;
using QLDA.WebApi.Models.TepDinhKems;
using SequentialGuid;

namespace QLDA.WebApi.Models.DeXuatNhuCauKinhPhiNams;

public class DeXuatNhuCauKinhPhiNamModel : IHasKey<Guid?>, IMustHaveId<Guid>, IMayHaveTepDinhKemModel {
    [DefaultValue(null)] public Guid? Id { get; set; }

    public Guid GetId() {
        Id ??= SequentialGuidGenerator.Instance.NewGuid();
        return (Guid)Id;
    }

    public string? So { get; set; }
    public DateTimeOffset? NgayKeHoach { get; set; }
    public string? TrichYeu { get; set; }
    public long? TongKinhPhiDeXuat { get; set; }
    public string? GhiChu { get; set; }
    public string? MaTrangThai { get; set; }
    public int? TrangThaiId { get; set; }
    public string? TenTrangThai { get; set; }

    /// <summary>Danh sách Id DeXuatNhuCauKinhPhi được trình trong tổng hợp KP năm</summary>
    public List<Guid>? DanhSachDeXuat { get; set; }

    public List<TepDinhKemModel>? DanhSachTepDinhKem { get; set; }
}
