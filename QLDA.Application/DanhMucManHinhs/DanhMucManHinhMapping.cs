using QLDA.Application.DanhMucManHinhs.DTOs;

namespace QLDA.Application.DanhMucManHinhs;

public static class DanhMucManHinhMapping {


    public static DanhMucManHinhDto ToDto(this DanhMucManHinh entity)
        => new() {
            Id = entity.Id,
            Ten = entity.Ten ?? string.Empty,
            Label = entity.Label,
            Title = entity.Title,
            Used = entity.Used,
        };
    public static DanhMucManHinh ToEntity(this DanhMucManHinhInsertDto dto)
        => new() {
            Ten = dto.Ten,
            Label = dto.Label,
            Title = dto.Title,
            Used = dto.Used
        };
    public static void Update(this DanhMucManHinh entity, DanhMucManHinhUpdateDto dto) {
        entity.Ten = dto.Ten;
        entity.Label = dto.Label;
        entity.Title = dto.Title;
        entity.Used = dto.Used;
    }

}