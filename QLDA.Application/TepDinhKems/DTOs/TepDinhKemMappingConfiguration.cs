using QLDA.Domain.Enums;

namespace QLDA.Application.TepDinhKems.DTOs;

public static class TepDinhKemMappingConfiguration {
    public static List<TepDinhKemDto> ToDtos(this IEnumerable<TepDinhKem> danhSachTepDinhKem)
        => [.. danhSachTepDinhKem.Select(o => o.ToDto())];

    public static TepDinhKemDto ToDto(this TepDinhKem entity)
        => new() {
            Id = entity.Id,
            ParentId = entity.ParentId,
            GroupId = entity.GroupId,
            GroupType = entity.GroupType,
            Path = entity.Path,
            Size = entity.Size,
            Type = entity.Type,
            FileName = entity.FileName,
            OriginalName = entity.OriginalName,
        };
    private static TepDinhKem ToEntity(this TepDinhKemInsertDto insertDto, Guid groupId, EGroupType groupType = EGroupType.None)
    => new() {
        Id = GuidExtensions.GetSequentialGuidId(),
        ParentId = insertDto.ParentId,
        GroupId = groupId.ToString(),
        GroupType = groupType.ToString(),
        Type = insertDto.Type,
        FileName = insertDto.FileName,
        OriginalName = insertDto.OriginalName,
        Path = insertDto.Path,
        Size = insertDto.Size,
    };
    private static TepDinhKem ToEntity(this TepDinhKemInsertDto insertDto, Guid groupId, string groupType = "None")
    => new() {
        Id = GuidExtensions.GetSequentialGuidId(),
        ParentId = insertDto.ParentId,
        GroupId = groupId.ToString(),
        GroupType = groupType,
        Type = insertDto.Type,
        FileName = insertDto.FileName,
        OriginalName = insertDto.OriginalName,
        Path = insertDto.Path,
        Size = insertDto.Size,
    };
    private static TepDinhKem ToEntity(this TepDinhKemInsertOrUpdateDto insertOrUpdateDto, Guid groupId, EGroupType groupType = EGroupType.None)
    => new() {
        Id = insertOrUpdateDto.Id.GetId(),
        ParentId = insertOrUpdateDto.ParentId,
        GroupId = groupId.ToString(),
        GroupType = groupType.ToString(),
        Type = insertOrUpdateDto.Type,
        FileName = insertOrUpdateDto.FileName,
        OriginalName = insertOrUpdateDto.OriginalName,
        Path = insertOrUpdateDto.Path,
        Size = insertOrUpdateDto.Size,
    };
    private static TepDinhKem ToEntity(this TepDinhKemInsertOrUpdateDto insertOrUpdateDto, Guid groupId, string groupType = "None")
    => new() {
        Id = insertOrUpdateDto.Id.GetId(),
        ParentId = insertOrUpdateDto.ParentId,
        GroupId = groupId.ToString(),
        GroupType = groupType,
        Type = insertOrUpdateDto.Type,
        FileName = insertOrUpdateDto.FileName,
        OriginalName = insertOrUpdateDto.OriginalName,
        Path = insertOrUpdateDto.Path,
        Size = insertOrUpdateDto.Size,
    };
    public static IEnumerable<TepDinhKem> ToEntities(
        this List<TepDinhKemInsertDto> dtos,
        Guid groupId,
        EGroupType groupType = EGroupType.None
    )
    => dtos.Select(m => ToEntity(m, groupId, groupType));
    public static IEnumerable<TepDinhKem> ToEntities(
        this List<TepDinhKemInsertDto> dtos,
        Guid groupId,
        string groupType = "None"
    )
    => dtos.Select(m => ToEntity(m, groupId, groupType));
    public static IEnumerable<TepDinhKem> ToEntities(
       this List<TepDinhKemInsertOrUpdateDto> dtos,
       Guid groupId,
       EGroupType groupType = EGroupType.None
   )
   => dtos.Select(m => ToEntity(m, groupId, groupType));
    public static IEnumerable<TepDinhKem> ToEntities(
       this List<TepDinhKemInsertOrUpdateDto> dtos,
       Guid groupId,
       string groupType = "None"
   )
   => dtos.Select(m => ToEntity(m, groupId, groupType));

    public static IEnumerable<TepDinhKem> ToEntities(
        this List<TepDinhKemDto> dtos,
        Guid groupId,
        EGroupType groupType = EGroupType.None
    )
    {
        foreach (var dto in dtos)
        {
            yield return new TepDinhKem
            {
                Id = dto.Id ?? GuidExtensions.GetSequentialGuidId(),
                ParentId = dto.ParentId,
                GroupId = groupId.ToString(),
                GroupType = groupType.ToString(),
                Type = dto.Type,
                FileName = dto.FileName,
                OriginalName = dto.OriginalName,
                Path = dto.Path,
                Size = dto.Size,
            };
        }
    }

    public static IEnumerable<TepDinhKem> ToEntities( this List<TepDinhKemDto> dtos, Guid groupId, string groupType = "None" )
    {
        foreach (var dto in dtos)
        {
            yield return new TepDinhKem
            {
                Id = dto.Id ?? GuidExtensions.GetSequentialGuidId(),
                ParentId = dto.ParentId,
                GroupId = groupId.ToString(),
                GroupType = groupType,
                Type = dto.Type,
                FileName = dto.FileName,
                OriginalName = dto.OriginalName,
                Path = dto.Path,
                Size = dto.Size,
            };
        }
    }
}