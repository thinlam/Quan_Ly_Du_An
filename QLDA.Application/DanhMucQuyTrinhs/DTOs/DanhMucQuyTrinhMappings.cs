
namespace QLDA.Application.DanhMucQuyTrinhs.DTOs;

public static class DanhMucQuyTrinhMappings {
    public static DanhMucQuyTrinhDto ToDto(this DanhMucQuyTrinh entity)
        => new DanhMucQuyTrinhDto() {
            Id = entity.Id,
            Ma = entity.Ma ?? string.Empty,
            MoTa = entity.MoTa ?? string.Empty,
            Stt = entity.Stt,
            Used = entity.Used,
            Ten = entity.Ten ?? string.Empty,
            MacDinh = entity.MacDinh,
        };

    public static List<DanhMucQuyTrinhDto> ToDtos(this List<DanhMucQuyTrinh> entities)
        => [.. entities.Select(e => e.ToDto())];

    public static DanhMucQuyTrinhDto ToDto(this DanhMucQuyTrinhDto dto)
        => new DanhMucQuyTrinhDto() {
            Id = dto.Id,
            Ma = dto.Ma,
            Ten = dto.Ten,
            MoTa = dto.MoTa,
            Stt = dto.Stt,
        };
}