namespace QLDA.Application.KySos.DTOs;

public class KySoInsertDto {
    public long? ChuSoHuuId { get; set; }

    public string? Email { get; set; }

    public int? ChucVuId { get; set; }

    public int? PhamVi { get; set; }

    public int? PhongBanId { get; set;}

    public string? SerialChungThu { get; set;}

    public string? ToChucCap { get; set;}

    public DateOnly? HieuLucTu { get; set;}

    public DateOnly? HieuLucDen { get; set;}

    public int? PhuongThucKySoId { get; set;}
}