using QLDA.Application.Common.Interfaces;

namespace QLDA.Application.KySos.DTOs;

public record NoiDungDaKySearchDto : CommonSearchDto {
    /// <summary>Người ký — UserPortalId (map TepDinhKem.CreatedBy).</summary>
    public long? CreateUserId { get; set; }
}
