using QLDA.Application.Common.Interfaces;
using QLDA.Application.TepDinhKems.DTOs;


namespace QLDA.Application.HoSoDeXuatCapDoCntts.DTOs;

public class HoSoDeXuatCapDoCnttInsertDto : IMayHaveTepDinhKemDto {
    public Guid DuAnId { get; set; }          // Ẩn trên UI, truyền từ context
    public int? BuocId { get; set; }          // Ẩn trên UI, truyền từ context
    public int? TrangThaiId { get; set; }     
    public int? CapDoId { get; set; }
    public DateOnly? NgayTrinh { get; set; }
    public int? DonViChuTriId { get; set; }
    public string? NoiDungDeNghi { get; set; }
    public string? NoiDungBaoCao { get; set; }
    public string? NoiDungDuThao { get; set; }
    public List<TepDinhKemDto>? DanhSachTepDinhKem { get; set; }
}