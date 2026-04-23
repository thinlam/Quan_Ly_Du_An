namespace QLDA.Application.DanhMucBuocs.DTOs;

public static class StepMappingMappings {
    public static StepDto ToStepDto(this DanhMucBuoc entity)
        => new StepDto() {
            Id = entity.Id,
            BuocId = entity.Id,
            Ten = entity.Ten ?? string.Empty,
            OriginalTen = entity.Ten ?? string.Empty,
            Path = entity.Path,
            ParentId = entity.ParentId ?? 0,
            SoNgayThucHien = entity.SoNgayThucHien,
            Level = entity.Level,
            Stt = entity.Stt ?? 0,
            PartialView = entity.PartialView,
            BuocManHinhs = entity.BuocManHinhs?
                .OrderBy(e => e.Stt)
                .ToList(),
        };

    public static List<StepDto> ToSteps(this List<DanhMucBuoc> entities)
        => [.. entities.Select(e => e.ToStepDto())];
    public static StepDto ToStepDto(this DuAnBuoc entity)
        => new StepDto() {
            Id = entity.Id,
            BuocId = entity.BuocId,
            Ten = entity.TenBuoc ?? entity.Buoc!.Ten ?? string.Empty,
            ParentId = entity.Buoc!.ParentId ?? 0,
            Path = entity.Buoc.Path,
            Level = entity.Buoc.Level,
            Stt = entity.Buoc.Stt ?? 0,
            PartialView = entity.Buoc.PartialView,
            BuocManHinhs = entity.Buoc.BuocManHinhs?
                .OrderBy(e => e.Stt)
                .ToList(),
            Phase = entity.Buoc.GiaiDoan?.ToPhaseDto()
        };
    public static PhaseDto ToPhaseDto(this DanhMucGiaiDoan entity)
        => new PhaseDto() {
            Id = entity.Id,
            Stt = entity.Stt ?? 0,
            Ten = entity.Ten,
        };

    public static List<StepDto> ToSteps(this List<DuAnBuoc> entities)
        => [.. entities.Select(e => e.ToStepDto())];
}