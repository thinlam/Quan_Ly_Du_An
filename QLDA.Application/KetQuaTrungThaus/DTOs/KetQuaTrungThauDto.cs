using QLDA.Application.Common.Interfaces;
using QLDA.Application.TepDinhKems.DTOs;
using QLDA.Domain.Interfaces;

namespace QLDA.Application.KetQuaTrungThaus.DTOs;

public class KetQuaTrungThauDto : IHasKey<Guid>,
    IMayHaveTepDinhKemDto,
    ITienDo,
    ITrichYeu {
    public Guid Id { get; set; }
    public Guid DuAnId { get; set; }
    public int? BuocId { get; set; }
    public Guid GoiThauId { get; set; }
    public string? NoiDung { get; set; }
    public long GiaTriTrungThau { get; set; }
    public Guid? DonViTrungThauId { get; set; }
    public int? SoNgayTrienKhai { get; set; }
    public string? TrichYeu { get; set; }
    public int? LoaiGoiThauId { get; set; }
    public DateTimeOffset? NgayEHSMT { get; set; }
    public DateTimeOffset? NgayMoThau { get; set; }

    /// <summary>
    /// Số ngày thực hiện hợp đồng
    /// </summary>
    public int? SoNgayThucHienHopDong { get; set; }

    #region Issue 9208
    /// <summary>
    /// Số quyết định
    /// </summary>
    public string? SoQuyetDinh { get; set; }
    /// <summary>
    /// Ngày quyết định
    /// </summary>
    public DateTimeOffset? NgayQuyetDinh { get; set; }
    #endregion
    public List<TepDinhKemDto>? DanhSachTepDinhKem { get; set; }
}