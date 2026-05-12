// Re-export DTOs from Domain layer for Application layer use
// This maintains backward compatibility and follows Clean Architecture
namespace QLDA.Domain.DTOs;

public record TinhHinhDauThauSearchDto : AggregateRootPagination
{
        public int? Nam { get; set; }
        public int? GiaiDoanId { get; set; }
        public int TrangThai { get; set; }
        public Guid? DuAnId { get; set; }
}
