using System.ComponentModel;
using QLDA.Domain.Interfaces;
using QLDA.WebApi.Models.DeXuatNhuCauKinhPhis;
using QLDA.WebApi.Models.TepDinhKems;
using SequentialGuid;

namespace QLDA.WebApi.Models.DeXuatNhuCauKinhPhiNams;

public class DeXuatNhuCauKinhPhiNamModel : IHasKey<Guid?>, IMustHaveId<Guid>, IMayHaveTepDinhKemModel{
    [DefaultValue(null)] public Guid? Id { get; set; }

    /// <summary>
    /// Nếu có id => cập nhật, ngược lại là tạo mới
    /// </summary>
    /// <returns></returns>
    public Guid GetId() {
        Id ??= SequentialGuidGenerator.Instance.NewGuid();
        return (Guid)Id;
    }

    public Guid SetId() {
        
        return SequentialGuidGenerator.Instance.NewGuid();
    }
    public string? So { get; set; }
    public DateTimeOffset? NgayKeHoach { get; set; }
    public string? TrichYeu { get; set; }
    public long? TongKinhPhiDeXuat { get; set; }
    public string? GhiChu { get; set; }
    public string? MaTrangThai { get; set; }
    public int? TrangThaiId { get; set; }
    public string? TenTrangThai { get; set; }
    public List<DeXuatNhuCauKinhPhiModel>? DanhSachDeXuat { get; set; }
    public List<TepDinhKemModel>? DanhSachTepDinhKem { get; set; }

}