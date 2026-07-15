using System.ComponentModel;
using QLDA.Domain.Interfaces;
using QLDA.WebApi.Models.TepDinhKems;
using SequentialGuid;

namespace QLDA.WebApi.Models.VanBanPhapLys;

public class VanBanPhapLyModel : IHasKey<Guid?>, IMustHaveId<Guid>, IMayHaveTepDinhKemModel, ITienDo {
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

    /// <summary>
    /// Tên dự án
    /// </summary>
    public int? BuocId { get; set; }

    public Guid DuAnId { get; set; }
    public string? SoVanBan { get; set; } = string.Empty;
    public string? CoQuanQuyetDinh { get; set; } = string.Empty;
    public DateTimeOffset? NgayVanBan { get; set; }
    public int? LoaiVanBanId { get; set; }
    public DateTimeOffset? NgayKy { get; set; }
    public string? NguoiKy { get; set; } = string.Empty;
    public string? TrichYeu { get; set; }
    /// <summary>
    /// Danh mục chức vụ
    /// </summary>
    public int? ChucVuId { get; set; }

    public List<TepDinhKemModel>? DanhSachTepDinhKem { get; set; }
}