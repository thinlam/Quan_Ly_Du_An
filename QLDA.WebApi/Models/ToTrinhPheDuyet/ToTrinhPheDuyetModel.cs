using System.ComponentModel;
using QLDA.Domain.Interfaces;
using QLDA.WebApi.Models.TepDinhKems;
using SequentialGuid;

namespace QLDA.WebApi.Models.ToTrinhPheDuyets;

public class ToTrinhPheDuyetModel : IHasKey<Guid?>, IMustHaveId<Guid>, IMayHaveTepDinhKemModel, ITienDo{
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
    public string So { get; set; } = string.Empty;
    public DateTimeOffset? Ngay { get; set; } 
    public string? TrichYeu { get; set; } = string.Empty;
    public string Loai { get; set; } = string.Empty;// Loại phê duyệt
    public List<TepDinhKemModel>? DanhSachTepDinhKem { get; set; }
}