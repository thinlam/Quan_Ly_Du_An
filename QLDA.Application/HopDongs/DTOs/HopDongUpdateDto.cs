
using BuildingBlocks.CrossCutting.ExtensionMethods;
using QLDA.Application.Common.Interfaces;
using QLDA.Application.TepDinhKems.DTOs;

namespace QLDA.Application.HopDongs.DTOs;

public class HopDongUpdateDto : IMayHaveTepDinhKemInsertOrUpdateDto {
    public Guid Id { get; set; }
    public Guid GoiThauId { get; set; }
    public string? Ten { get; set; }
    public string? SoHopDong { get; set; }
    public string? NoiDung { get; set; }
    public Guid? DonViThucHienId { get; set; }
    public DateTimeOffset? NgayKy { get; set; }
    public long? GiaTri { get; set; }

    /// <summary>
    /// Ngày hiệu lực (DateOnly). Entity lưu DateTimeOffset.
    /// </summary>
    public DateOnly? NgayHieuLuc { get; set; }

    /// <summary>
    /// Ngày dự kiến kết thúc hợp đồng (DateOnly). Entity lưu DateTimeOffset.
    /// Tự động tính từ Ngày hiệu lực + Thời gian thực hiện hợp đồng, cho phép chỉnh sửa.
    /// </summary>
    public DateOnly? NgayDuKienKetThucHopDong { get; set; }

    /// <summary>
    /// Ngày dự kiến kết thúc gói thầu (DateOnly). Entity lưu DateTimeOffset.
    /// Tự động tính từ Ngày hiệu lực + Thời gian thực hiện gói thầu, cho phép chỉnh sửa.
    /// </summary>
    public DateOnly? NgayDuKienKetThucGoiThau { get; set; }

    public int? LoaiHopDongId { get; set; }

    /// <summary>
    /// Là hợp đồng hay biên bản giao nhiệm vụ
    /// </summary>
    [DefaultValue(true)]
    public bool IsBienBan { get; set; } = true;

    public List<TepDinhKemInsertOrUpdateDto>? DanhSachTepDinhKem { get; set; }
}