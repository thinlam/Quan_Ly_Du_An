using BuildingBlocks.Domain.Interfaces;

namespace BuildingBlocks.Domain.Entities;

public class DmChucVu : IHasKey<long>, IAggregateRoot
{
    public long Id { get; set; }
    public string Ten { get; set; } = string.Empty;
    public bool Used { get; set; } = true;

}
