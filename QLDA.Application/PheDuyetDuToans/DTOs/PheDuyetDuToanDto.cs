using QLDA.Application.Common.Interfaces;
using QLDA.Application.TepDinhKems.DTOs;
using QLDA.Domain.Constants;
using QLDA.Domain.Interfaces;
using SequentialGuid;

namespace QLDA.Application.PheDuyetDuToans.DTOs;

public class PheDuyetDuToanDto : IHasKey<Guid?>, IMustHaveId<Guid>, IMayHaveTepDinhKemDto, ITienDo {
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

    public Guid DuAnId { get; set; }
    public int? BuocId { get; set; }
    public string? SoVanBan { get; set; }
    public DateTimeOffset? NgayKy { get; set; }
    public string? NguoiKy { get; set; }
    public int? ChucVuId { get; set; }
    public long? GiaTriDuThau { get; set; }
    public string? TrichYeu { get; set; }
    public int TrangThaiId { get; set; } = 1;
    public string? MaTrangThai { get; set; }
    public string? TenTrangThai { get; set; }
    /// <summary>
    /// Dùng để phân biệt với các bước khác, nếu có gửi duyệt thì sẽ có giá trị true, ngược lại là false.
    /// </summary>
    public bool IsSend => MaTrangThai == TrangThaiPheDuyetDuToanCodes.DaTrinh;

    /// <summary>
    /// USER_MASTER.UserPortalId
    /// </summary>
    public long? NguoiXuLyId { get; set; }

    /// <summary>
    /// USER_MASTER.UserPortalId
    /// </summary>
    public long? NguoiGiaoViecId { get; set; }

    public List<TepDinhKemDto>? DanhSachTepDinhKem { get; set; }
}