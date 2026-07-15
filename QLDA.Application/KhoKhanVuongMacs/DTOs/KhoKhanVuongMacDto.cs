using QLDA.Application.Common.Interfaces;
using QLDA.Application.TepDinhKems.DTOs;
using QLDA.Domain.Interfaces;
using SequentialGuid;

namespace QLDA.Application.KhoKhanVuongMacs.DTOs;

public class KhoKhanVuongMacDto : IHasKey<Guid?>, IMustHaveId<Guid>, IMayHaveTepDinhKemDto, ITienDo {
    [DefaultValue(null)] public Guid? Id { get; set; }

    public Guid GetId() {
        Id ??= SequentialGuidGenerator.Instance.NewGuid();
        return (Guid)Id;
    }

    public Guid DuAnId { get; set; }
    /// <summary>
    /// Dự án bước id
    /// </summary>
    public int? BuocId { get; set; }
    /// <summary>
    /// Ngày báo cáo
    /// </summary>
    public DateOnly? Ngay { get; set; }
    public string? NoiDung { get; set; }
    /// <summary>
    /// Tình trạng khó khăn| trạng thái xử lý
    /// </summary>
    public int? TinhTrangId { get; set; }
    /// <summary>
    /// Mức độ/ cấp độ khó khăn
    /// </summary>
    public int? MucDoKhoKhanId { get; set; }
    /// <summary>
    /// Hướng xử lý
    /// </summary>
    public string? HuongXuLy { get; set; }
    public List<TepDinhKemDto>? DanhSachTepDinhKem { get; set; }
    /// <summary>
    /// Kết quả xử lý
    /// </summary>
    public KetQuaXuLyDto? KetQua { get; set; }

    #region Thông tin dự án
    /// <summary>
    /// Ngày bắt đầu dự án
    /// </summary>
    public DateTimeOffset? NgayBatDau { get; set; }

    /// <summary>
    /// Loại dự án
    /// </summary>
    public int? LoaiDuAnId { get; set; }
    #region Lãnh đão + Phòng ban phụ trách
    public long? LanhDaoPhuTrachId { get; set; }
    public long? DonViPhuTrachChinhId { get; set; }
    public long? DonViPhoiHopId { get; set; }

    #endregion
    #endregion
}
