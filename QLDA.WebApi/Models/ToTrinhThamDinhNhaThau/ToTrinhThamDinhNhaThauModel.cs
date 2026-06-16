using System.ComponentModel;
using QLDA.Domain.Interfaces;
using QLDA.WebApi.Models.TepDinhKems;
using QLDA.WebApi.Models.KetQuaThamDinhNhaThaus;
using SequentialGuid;

namespace QLDA.WebApi.Models.ToTrinhThamDinhNhaThaus;

public class ToTrinhThamDinhNhaThauModel : IHasKey<Guid?>, IMustHaveId<Guid>, IMayHaveTepDinhKemModel, ITienDo{
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
    public DateTimeOffset NgayTrinh     { get; set; } 
    public string? TrichYeu { get; set; } = string.Empty;
    public int? TrangThaiDangTaiId { get; set; }
    public bool? DaThamDinh { get; set; }
    public List<KetQuaThamDinhNhaThauModel>? DanhSachNhaThaus { get; set; }
    public List<TepDinhKemModel>? DanhSachTepDinhKem { get; set; }
    public List<TepDinhKemModel>? DanhSachTepThamDinh { get; set; }
}