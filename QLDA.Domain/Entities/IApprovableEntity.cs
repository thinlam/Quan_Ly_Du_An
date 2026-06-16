using QLDA.Domain.Enums;

namespace QLDA.Domain.Entities;

public interface IApprovableEntity
{

    public Guid DuAnId { get; set; }
    public int? BuocId { get; set; }
    int? TrangThaiId { get; set; }
}