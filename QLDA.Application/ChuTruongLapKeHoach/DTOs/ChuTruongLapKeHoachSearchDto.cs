//using QLDA.Application.Common.Interfaces;
using QLDA.Application.Common.Interfaces;

namespace QLDA.Application.ChuTruongLapKeHoachs.DTOs;

public record ChuTruongLapKeHoachSearchDto : CommonSearchDto
{

     /// <summary>
    /// em comment vì khi e test nó lỗi "ChuTruongLapKeHoachSearchDto đang kế thừa CommonSearchDto, mà trong CommonSearchDto khả năng cao đã có sẵn DuAnId rồi" nên em comment lại để nếu có gì mn chỉ cần xóa dấu // 
    /// </summary>
    //public Guid DuAnId { get; set; } 
   // public int? LoaiDeXuat { get; set; }
  //  public string? SoToTrinh { get; set; }
   // public string? TrichYeu { get; set; }
    //public string? ButPhe { get; set; }
    /// <summary>
    /// Loại dự án theo năm - tài chính
    /// </summary>
    /// <remarks>PMIS #9609</remarks>
    public int? LoaiDuAnTheoNamId { get; set; }
}