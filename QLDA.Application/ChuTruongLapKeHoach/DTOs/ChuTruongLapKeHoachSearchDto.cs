using QLDA.Application.Common.Interfaces;
using QLDA.Application.Common.Interfaces;

namespace QLDA.Application.ChuTruongLapKeHoachs.DTOs;

public record ChuTruongLapKeHoachSearchDto : CommonSearchDto
{

    public Guid DuAnId { get; set; }
   // public int? LoaiDeXuat { get; set; }
  //  public string? SoToTrinh { get; set; }
   // public string? TrichYeu { get; set; }
    //public string? ButPhe { get; set; }
}