using QLDA.Application.Common.Interfaces;
using QLDA.Application.TepDinhKems.DTOs;

namespace QLDA.Application.VanBanPhapLys.DTOs;

public class VanBanPhapLyDto : IHasKey<Guid?>, IMayHaveTepDinhKemDto {
    [DefaultValue(null)] public Guid? Id { get; set; }

    /// <summary>
    /// Tên dự án
    /// </summary>
    public int? BuocId { get; set; }

    public Guid DuAnId { get; set; }
    public DateTimeOffset? NgayVanBan { get; set; }
    public string? SoVanBan { get; set; } = string.Empty;
    public string? CoQuanQuyetDinh { get; set; } = string.Empty;
    public string? TrichYeu { get; set; } = string.Empty;
    public int? LoaiVanBanId { get; set; }
    public DateTimeOffset? NgayKy { get; set; }
    public string? NguoiKy { get; set; } = string.Empty;

    /// <summary>
    /// Danh mục chức vụ
    /// </summary>
    public int? ChucVuId { get; set; }

    public List<TepDinhKemDto>? DanhSachTepDinhKem { get; set; }
}