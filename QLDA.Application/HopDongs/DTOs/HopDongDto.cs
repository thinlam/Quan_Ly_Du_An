using BuildingBlocks.CrossCutting.ExtensionMethods;
using QLDA.Application.Common.Interfaces;
using QLDA.Application.TepDinhKems.DTOs;
using QLDA.Domain.Interfaces;
using SequentialGuid;

namespace QLDA.Application.HopDongs.DTOs;

public class HopDongDto : IHasKey<Guid?>, IMustHaveId<Guid>, IMayHaveTepDinhKemDto, ITienDo {
    [DefaultValue(null)] public Guid? Id { get; set; }

    public Guid GetId() {
        Id ??= SequentialGuidGenerator.Instance.NewGuid();
        return (Guid)Id;
    }

    public Guid DuAnId { get; set; }
    public int? BuocId { get; set; }
    public Guid? GoiThauId { get; set; }

    #region Thông tin thanh toán

    public List<Guid>? ThanhToanIds { get; set; }
    #endregion

    #region Thông tin tạm ứng

    public Guid? TamUngId { get; set; }
    public string? SoPhieuChi { get; set; }

    #endregion

    public string? Ten { get; set; }
    public string? SoHopDong { get; set; }
    public string? NoiDung { get; set; }
    public DateTimeOffset? NgayKy { get; set; }
    public long? GiaTri { get; set; }

    /// <summary>
    /// Ngày hiệu lực hợp đồng (DateOnly). Entity lưu DateTimeOffset.
    /// </summary>
    public DateOnly? NgayHieuLuc { get; set; }

    /// <summary>
    /// Ngày dự kiến kết thúc hợp đồng (DateOnly). Entity lưu DateTimeOffset.
    /// </summary>
    public DateOnly? NgayDuKienKetThucHopDong { get; set; }

    /// <summary>
    /// Ngày dự kiến kết thúc gói thầu (DateOnly). Entity lưu DateTimeOffset.
    /// </summary>
    public DateOnly? NgayDuKienKetThucGoiThau { get; set; }

    public int? LoaiHopDongId { get; set; }
    public bool IsBienBan { get; set; }

    /// <summary>
    /// Đơn vị thực hiện
    /// </summary>
    public Guid? DonViThucHienId { get; set; }



    public List<TepDinhKemDto>? DanhSachTepDinhKem { get; set; }
}