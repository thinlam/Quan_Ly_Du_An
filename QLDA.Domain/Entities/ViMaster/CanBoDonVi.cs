namespace QLDA.Domain.Entities.ViMaster;

public class CanBoDonVi : IHasKey<long>, IAggregateRoot {
    public long Id { get; set; }

    public long? CanBoId { get; set; }

    public long? DonViId { get; set; }

    public long? ChucVuId { get; set; }

    public bool? LaChucVuChinh { get; set; }

}