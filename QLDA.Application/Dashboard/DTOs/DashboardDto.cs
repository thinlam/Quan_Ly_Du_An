// Re-export DTOs from Domain layer for Application layer use
// This maintains backward compatibility and follows Clean Architecture
global using QLDA.Domain.DTOs;

namespace QLDA.Application.Dashboard.DTOs;
public record DashboardGiaiNganBaseDto {
    public Guid DuAnId { get; set; }
    public string TenDuAn { get; set; } = string.Empty;

    public int NguonVonId { get; set; }
    public string TenNguonVon { get; set; } = string.Empty;

    public decimal KeHoachVon { get; set; }
    public decimal GiaiNgan { get; set; }
}
// Empty namespace - all DTOs are defined in QLDA.Domain.DTOs
