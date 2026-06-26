namespace QLDA.Domain.Entities;

/// <summary>
/// Cấu hình người dùng mặc định theo phòng ban
/// </summary>
public class NguoiDungMacDinhTheoPhong : Entity<Guid>, IAggregateRoot
{
    public long PhongBanId { get; set; }
    public long NguoiDungId { get; set; }
}
