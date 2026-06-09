using System.ComponentModel;
using QLDA.Domain.Interfaces;
using QLDA.WebApi.Models.TepDinhKems;
using SequentialGuid;

namespace QLDA.WebApi.Models.ChuTruongLapKeHoachs;

public class ChuTruongLapKeHoachModel : IHasKey<Guid?>, IMustHaveId<Guid>, IMayHaveTepDinhKemModel, ITienDo{
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
 
    public int? BuocId { get; set; }
    public Guid DuAnId { get; set; }
    public string? SoToTrinh { get; set; } 
    public DateTimeOffset? NgayToTrinh { get; set; } 
    public string? TrichYeu { get; set; } = string.Empty;
    public string? ButPhe { get; set; } = string.Empty;
    public int? LoaiDeXuat { get; set; }
    public List<TepDinhKemModel>? DanhSachTepDinhKem { get; set; }
}