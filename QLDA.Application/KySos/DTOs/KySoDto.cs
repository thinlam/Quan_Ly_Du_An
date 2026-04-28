using QLDA.Domain.Enums;

namespace QLDA.Application.KySos.DTOs;

public class KySoDto {
    public Guid Id { get; set; }

    public long? ChuSoHuuId { get; set; }

    public string? Email { get; set; }

    public int? ChucVuId { get; set; }

    public string? TenChucVu { get; set; }

    public EPhamViKySo? PhamVi { get; set; }

    public int? PhongBanId { get; set; }

    public string? SerialChungThu { get; set; }

    public string? ToChucCap { get; set; }

    public DateTime? HieuLucTu { get; set; }

    public DateTime? HieuLucDen { get; set; }

    public int? PhuongThucKySoId { get; set; }

    public string? TenPhuongThucKySo { get; set; }
}