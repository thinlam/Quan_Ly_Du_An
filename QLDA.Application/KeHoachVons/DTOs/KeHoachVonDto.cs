namespace QLDA.Application.KeHoachVons.DTOs;

public class KeHoachVon : IHasKey<Guid> {

    public Guid Id { get; set;}

    public Guid DuAnId { get; set;}

    public Guid? NguonVonId { get; set;}

    public int Nam { get ; set ; }

    public decimal SoVon {get; set;}

    public decimal? SoVonDieuChinh { get; set;}

    public string? SoQuyetDinh { get; set;}

    public DateTimeOffset? NgayKy { get; set;}

    public string? GhiChu { get; set;}
}