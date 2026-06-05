using QLDA.Application.Common.Interfaces;
using QLDA.Application.TepDinhKems.DTOs;
using QLDA.Domain.Interfaces;

namespace QLDA.Application.DeXuatChuTruongMois.DTOs;

public class DeXuatChuTruongMoiInsertDto : IMayHaveTepDinhKemDto, ITienDo {
    [DefaultValue(null)] public Guid? Id { get; set; }
    public Guid DuAnId { get; set; }
    public int? BuocId { get; set; }
    public string? TomTatNoiDung { get; set; }

    public long? TongMucDauTu { get; set; }

    public DateTimeOffset? NgayBatDauDuKien { get; set; }

    public int? HinhThucDauTuId { get; set; }
    public long? LanhDaoPhuTrachId { get; set; }
    public long? NguoiXuLyChinhId { get; set; }
    public long? DonViPhuTrachChinhId { get; set; }
    public int? TrangThaiId { get; set; }
    public int? NamDeXuat { get; set; }
    public List<TepDinhKemDto>? DanhSachTepDinhKem { get; set; }
    public List<long>? DonViPhoiHopIds { get; set; }


}