using QLDA.Application.Common.Interfaces;
using QLDA.Application.TepDinhKems.DTOs;
using QLDA.Domain.Interfaces;

namespace QLDA.Application.DeXuatChuyenTieps.DTOs;

public class DeXuatChuyenTiepInsertDto : IMayHaveTepDinhKemDto, ITienDo {
    [DefaultValue(null)] public Guid? Id { get; set; }
    public Guid DuAnId { get; set; }
    public int? BuocId { get; set; }
    public long? SoLieuGiaiNgan { get; set; }
    public long? UocGiaiNgan { get; set; }
    public long? NhuCauKinhPhi { get; set; }
    public string? KhoiLuongThucTe { get; set; }
    public string? KhoiLuongDuKien { get; set; }
    public int? TrangThaiId { get; set; }
    public List<TepDinhKemDto>? DanhSachTepDinhKem { get; set; }


}