namespace QLDA.Domain.Entities;

public class QuyetDinhDuyetDuToanNguonVon : Entity<Guid>, IAggregateRoot
{
    public Guid QuyetDinhDuToanId { get; set; }
    public int NguonVonId { get; set; }
    public long GiaTri { get; set; }
    public int? Nam { get; set; }
    public QuyetDinhDuyetDuToan? QuyetDinhDuToan { get; set; }
}
