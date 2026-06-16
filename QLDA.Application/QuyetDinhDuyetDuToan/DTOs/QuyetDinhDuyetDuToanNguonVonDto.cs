using BuildingBlocks.Application.Common.Interfaces;
using SequentialGuid;

namespace QLDA.Application.QuyetDinhDuyetDuToans.DTOs;

public class QuyetDinhDuyetDuToanNguonVonDto: IHasKey<Guid?>, IMustHaveId<Guid>
{
    [DefaultValue(null)] public Guid? Id { get; set; }

    public Guid GetId()
    {
        Id ??= SequentialGuidGenerator.Instance.NewGuid();
        return (Guid)Id;
    }
    public int NguonVonId { get; set; }
    public long? GiaTri { get; set; }
    public int? Nam { get; set; }
}