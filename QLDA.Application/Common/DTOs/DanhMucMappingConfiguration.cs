namespace QLDA.Application.Common.DTOs;

public static class DanhMucMappingConfiguration {
    public static TDto ToDanhMucDto<TEntity, TKey, TDto>(this TEntity entity)
        where TEntity : DanhMuc<TKey>, IMayHaveStt
        where TDto : DanhMucDto<TKey>, new()
        => new() {
            Id = entity.Id,
            Ma = entity.Ma ?? string.Empty,
            Ten = entity.Ten ?? string.Empty,
            MoTa = entity.MoTa ?? string.Empty,
            Stt = entity.Stt,
            Used = entity.Used,
        };
}