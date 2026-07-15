namespace QLDA.Application.QuyetDinhLapBanQLDAs.DTOs;

/// <summary>
/// Read model chỉ dùng cho API in tờ trình lập Ban QLDA.
/// </summary>
public class QuyetDinhLapBanQldaPrintDto
{
    public Guid Id { get; set; }
    public Guid DuAnId { get; set; }
    public string? So { get; set; }
    public string? TrichYeu { get; set; }
    public string? SoDuThao { get; set; }
    public string? TrichYeuDuThao { get; set; }

    /// <summary>DuAn.LanhDaoPhuTrachId — thực chất là UserPortalId.</summary>
    public long? LanhDaoPhuTrachId { get; set; }

    /// <summary>user_master.HoTen join theo UserPortalId.</summary>
    public string? TenLanhDaoPhuTrach { get; set; }

    public List<ThanhVienBanQldaDto> ThanhViens { get; set; } = [];
}
