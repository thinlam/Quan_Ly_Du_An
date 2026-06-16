using System.ComponentModel;
using QLDA.Domain.Interfaces;
using QLDA.WebApi.Models.TepDinhKems;
using SequentialGuid;

namespace QLDA.WebApi.Models.ToTrinhKetQuaGoiThaus;

public class ToTrinhKetQuaGoiThauModel : IHasKey<Guid?>, IMustHaveId<Guid>, IMayHaveTepDinhKemModel, ITienDo{
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
    public DateTimeOffset? NgayTrinh     { get; set; } 
    public string? TrichYeu { get; set; } = string.Empty;
    public int? TrangThaiDangTaiId { get; set; }
    public List<GoiThauCboModel>? GoiThaus { get; set; }
    public List<TepDinhKemModel>? DanhSachTepDinhKem { get; set; }
}
public class GoiThauCboModel
{
    public Guid GoiThauId { get; set; }
    public string? TenGoiThau { get; set; }
}