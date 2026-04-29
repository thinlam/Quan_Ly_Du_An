namespace QLDA.Application.CauHinhVaiTroQuyens.DTOs;

/// <summary>
/// DTO for a single role-permission toggle row
/// </summary>
public class CauHinhVaiTroQuyenDto {
    public int Id { get; set; }
    public string VaiTro { get; set; } = string.Empty;
    public int QuyenId { get; set; }
    public string QuyenMa { get; set; } = string.Empty;
    public string QuyenTen { get; set; } = string.Empty;
    public string? NhomQuyen { get; set; }
    public bool KichHoat { get; set; }
}

/// <summary>
/// DTO for batch updating toggle states
/// </summary>
public class CauHinhVaiTroQuyenUpdateDto {
    public List<CauHinhVaiTroQuyenToggle> Items { get; set; } = [];
}

public class CauHinhVaiTroQuyenToggle {
    public int Id { get; set; }
    public bool KichHoat { get; set; }
}
