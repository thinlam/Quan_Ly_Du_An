using QLDA.Domain.Entities.DanhMuc;

namespace QLDA.Domain.Entities;

public class QuyetDinhDuyetDuToanChiPhi : Entity<Guid>, IAggregateRoot
{
    public Guid QuyetDinhDuToanId { get; set; }
    public string ChiPhi { get; set; } = string.Empty;
    public long GiaTri { get; set; }
    public QuyetDinhDuyetDuToan? QuyetDinhDuToan { get; set; }

}