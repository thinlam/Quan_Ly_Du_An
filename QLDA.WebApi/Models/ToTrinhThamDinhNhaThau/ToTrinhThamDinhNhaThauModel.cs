using System.ComponentModel;
using QLDA.Domain.Interfaces;
using QLDA.WebApi.Models.TepDinhKems;
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
    public Guid? NhaThauId { get; set; }
    public int? TrangThaiDangTaiId { get; set; }
    public List<TepDinhKemModel>? DanhSachTepDinhKem { get; set; }
    public List<TepDinhKemModel>? DanhSachTepThamDinh { get; set; }

    /// <summary>3 bước xử lý (Issue #179) — FE truyền trực tiếp, BE tự set Loai tương ứng.</summary>
    public ToTrinhThamDinhBuocXuLyModel? DoiChieu { get; set; }
    public ToTrinhThamDinhBuocXuLyModel? ThuongThao { get; set; }
    public ToTrinhThamDinhBuocXuLyModel? ThamDinh { get; set; }
}