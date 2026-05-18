using BuildingBlocks.Domain.Interfaces;

namespace QLDA.Domain.Entities;

public class DuAnBuocPhongBanPhoiHop : IJunctionEntity<int, long> {
    public int LeftId { get; set; }
    public long RightId { get; set; }
}