using BuildingBlocks.Application.Common.Interfaces;
using SequentialGuid;

namespace QLDA.Application.QuyetDinhDuyetDuToans.DTOs;

public class QuyetDinhDuyetDuToanChiPhiDto : IHasKey<Guid?>, IMustHaveId<Guid>
{
    [DefaultValue(null)]
    public Guid? Id { get; set; }

    public Guid GetId()
    {
        Id ??= SequentialGuidGenerator.Instance.NewGuid();
        return (Guid)Id;
    }
    public string? TenChiPhi { get; set; }
    public long? GiaTri { get; set; }
}