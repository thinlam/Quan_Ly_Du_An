using QLDA.Application.Common.Interfaces;
using QLDA.Application.TepDinhKems.DTOs;
using QLDA.Domain.Interfaces;

namespace QLDA.Application.GoiThaus.DTOs;

public class GoiThauDto : IHasKey<Guid>, IMayHaveTepDinhKemDto, ITienDo {
    public Guid Id { get; set; }
    public Guid DuAnId { get; set; }
    public Guid? KetQuaTrungThauId { get; set; }
    public Guid? HopDongId { get; set; }
    public int? BuocId { get; set; }
    public string? Ten { get; set; }
    public bool DaDuyet { get; set; } = true;
    public long? GiaTri { get; set; }
    public int? LoaiHopDongId { get; set; }
    public int? NguonVonId { get; set; }
    public string? ThoiGianHopDong { get; set; }
    public string? ThoiGianLuaNhaThau { get; set; }
    public int? PhuongThucLuaChonNhaThauId { get; set; }
    public Guid? KeHoachLuaChonNhaThauId { get; set; }
    public int? HinhThucLuaChonNhaThauId { get; set; }
    public string? TomTatCongViecChinhGoiThau { get; set; }
    public string? ThoiGianBatDauToChucLuaChonNhaThau { get; set; }
    public int? ThoiGianThucHienGoiThau { get; set; }
    public string? TuyChonMuaThem { get; set; }
    public string? GiamSatHoatDongDauThau { get; set; }


    public List<TepDinhKemDto>? DanhSachTepDinhKem { get; set; }
}