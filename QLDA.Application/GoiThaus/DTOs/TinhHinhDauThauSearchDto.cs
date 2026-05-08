// Re-export DTOs from Domain layer for Application layer use
// This maintains backward compatibility and follows Clean Architecture
namespace QLDA.Domain.DTOs;

public record TinhHinhDauThauSearchDto
{
        public int? Nam { get; set; }
        public int? GiaiDoanId { get; set; }
        public int TrangThai { get; set; }
        public Guid? DuAnId { get; set; }
        public int PageIndex { get; set; } = 1;
        public int PageSize { get; set; } = 10;

}
