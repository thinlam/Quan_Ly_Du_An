using QLDA.Application.Common.Interfaces;
using QLDA.Application.TepDinhKems.DTOs;
using QLDA.Domain.Interfaces;
using SequentialGuid;

namespace QLDA.Application.TongHopDeXuatChuTruongs.DTOs;

public class TongHopDeXuatChuTruongResponseDto
{
    public int TongDeXuatMoi { get; set; }
    public int TongDeXuatChuyenTiep { get; set; }
    public PaginatedList<TongHopDeXuatChuTruongDto> Data { get; set; }
}
    public class TongHopDeXuatChuTruongDto : IHasKey<Guid?>, IMustHaveId<Guid>, ITienDo {
    [DefaultValue(null)] public Guid? Id { get; set; }

    public Guid GetId() {
        Id ??= SequentialGuidGenerator.Instance.NewGuid();
        return (Guid)Id;
    }

    public Guid DuAnId { get; set; }
    public string TenDuAn { get; set; }
    public long? PhongBanPhuTrachId { get; set; }
    public string? TenPhongBanPhuTrach { get; set; }
    public int? BuocId { get; set; }
    public string? Loai { get; set; } // Moi or ChuyenTiep
    public string? TomTatNoiDung { get; set; }
    public int? TrangThaiId { get; set; }
    public string? MaTrangThai { get; set; }
    public string? TenTrangThai { get; set; }
    public string? LoaiTep { get; set; }
   // public IEnumerable<TepDinhKemDto> DanhSachTepDinhKem { get; set; }
}