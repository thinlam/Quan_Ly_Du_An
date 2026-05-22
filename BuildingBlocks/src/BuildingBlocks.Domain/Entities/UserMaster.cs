using BuildingBlocks.Domain.Interfaces;

namespace BuildingBlocks.Domain.Entities;

public class UserMaster : IHasKey<long>, IAggregateRoot
{
    public long Id { get; set; }

    public string? UserName { get; set; }

    public string? HoTen { get; set; }

    public long? PhongBanId { get; set; }

    public long? DonViId { get; set; }

    public long? UserPortalId { get; set; }

    public long? CanBoId { get; set; }

    public bool? LaDonViChinh { get; set; }

    public bool? Used { get; set; }

}
