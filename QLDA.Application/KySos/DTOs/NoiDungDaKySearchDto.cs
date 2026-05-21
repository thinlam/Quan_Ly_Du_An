using QLDA.Application.Common.Interfaces;

namespace QLDA.Application.KySos.DTOs;

public record NoiDungDaKySearchDto : CommonSearchDto {
    /// <summary>Người ký / người tạo bản ghi (map TepDinhKem.CreatedBy).</summary>
    public long? CreateUserId { get; set; }

    /// <summary>Loại — map TepDinhKem.GroupType. Null = KySo + NoiDungDaKySo.</summary>
    public string? GroupType { get; set; }
}
