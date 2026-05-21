namespace QLDA.Application.HoSoDeXuatCapDoCntts.DTOs;

public class HoSoDeXuatCapDoCnttUpdateModel {
    public Guid Id { get; set; }
    public Guid DuAnId { get; set; }
    public int? BuocId { get; set; }
    public int? TrangThaiId { get; set; }
    public int? CapDoId { get; set; }
    public DateOnly? NgayTrinh { get; set; }
    public int? DonViChuTriId { get; set; }
    public string? NoiDungDeNghi { get; set; }
    public string? NoiDungBaoCao { get; set; }
    public string? NoiDungDuThao { get; set; }
}