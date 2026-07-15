namespace QLDA.Domain.Entities.ViMaster;

public class DanhMucChucVuMaster
{
    public long ChucVuId { get; set; }

    public string? MaChucVu { get; set; }

    public string? TenChucVu { get; set; }

    public string? MoTa { get; set; }

    public bool? Used { get; set; }

    public long? CapDonViId { get; set; }

    public int? ThuTuHienThi { get; set; }
   
}
