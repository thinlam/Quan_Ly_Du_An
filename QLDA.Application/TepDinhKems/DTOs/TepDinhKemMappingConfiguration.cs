using QLDA.Application.Common;
using QLDA.Domain.Enums;

namespace QLDA.Application.TepDinhKems.DTOs;

public static class TepDinhKemMappingConfiguration {
    public static List<TepDinhKemDto> ToDtos(this IEnumerable<Attachment> danhSachTepDinhKem)
        => [.. danhSachTepDinhKem.Select(o => o.ToDto())];

    public static TepDinhKemDto ToDto(this Attachment entity)
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
    private static Attachment ToEntity(this TepDinhKemInsertDto insertDto, Guid groupId, EGroupType groupType = EGroupType.None)
    => new() {
        Id = GuidExtensions.GetSequentialGuidId(),
        ParentId = insertDto.ParentId,
        GroupId = groupId.ToString(),
        GroupType = groupType.ToString().ResolveSignedGroupType(insertDto.ParentId != null),
        Type = insertDto.Type,
        FileName = insertDto.FileName,
        OriginalName = insertDto.OriginalName,
        Path = insertDto.Path,
        Size = insertDto.Size,
    };
    private static Attachment ToEntity(this TepDinhKemInsertDto insertDto, Guid groupId, string groupType = "None")
    => new() {
        Id = GuidExtensions.GetSequentialGuidId(),
        ParentId = insertDto.ParentId,
        GroupId = groupId.ToString(),
        GroupType = groupType.ResolveSignedGroupType(insertDto.ParentId != null),
        Type = insertDto.Type,
        FileName = insertDto.FileName,
        OriginalName = insertDto.OriginalName,
        Path = insertDto.Path,
        Size = insertDto.Size,
    };
    private static Attachment ToEntity(this TepDinhKemInsertOrUpdateDto insertOrUpdateDto, Guid groupId, EGroupType groupType = EGroupType.None)
    => new() {
        Id = insertOrUpdateDto.Id.GetId(),
        ParentId = insertOrUpdateDto.ParentId,
        GroupId = groupId.ToString(),
        GroupType = groupType.ToString().ResolveSignedGroupType(insertOrUpdateDto.ParentId != null),
        Type = insertOrUpdateDto.Type,
        FileName = insertOrUpdateDto.FileName,
        OriginalName = insertOrUpdateDto.OriginalName,
        Path = insertOrUpdateDto.Path,
        Size = insertOrUpdateDto.Size,
    };
    private static Attachment ToEntity(this TepDinhKemInsertOrUpdateDto insertOrUpdateDto, Guid groupId, string groupType = "None")
    => new() {
        Id = insertOrUpdateDto.Id.GetId(),
        ParentId = insertOrUpdateDto.ParentId,
        GroupId = groupId.ToString(),
        GroupType = groupType.ResolveSignedGroupType(insertOrUpdateDto.ParentId != null),
        Type = insertOrUpdateDto.Type,
        FileName = insertOrUpdateDto.FileName,
        OriginalName = insertOrUpdateDto.OriginalName,
        Path = insertOrUpdateDto.Path,
        Size = insertOrUpdateDto.Size,
    };
    public static IEnumerable<Attachment> ToEntities(
        this List<TepDinhKemInsertDto> dtos,
        Guid groupId,
        EGroupType groupType = EGroupType.None
    )
    => dtos.Select(m => ToEntity(m, groupId, groupType));
    public static IEnumerable<Attachment> ToEntities(
        this List<TepDinhKemInsertDto> dtos,
        Guid groupId,
        string groupType = "None"
    )
    => dtos.Select(m => ToEntity(m, groupId, groupType));
    public static IEnumerable<Attachment> ToEntities(
       this List<TepDinhKemInsertOrUpdateDto> dtos,
       Guid groupId,
       EGroupType groupType = EGroupType.None
   )
   => dtos.Select(m => ToEntity(m, groupId, groupType));
    public static IEnumerable<Attachment> ToEntities(
       this List<TepDinhKemInsertOrUpdateDto> dtos,
       Guid groupId,
       string groupType = "None"
   )
   => dtos.Select(m => ToEntity(m, groupId, groupType));

    public static IEnumerable<Attachment> ToEntities(
        this List<TepDinhKemDto> dtos,
        Guid groupId,
        EGroupType groupType = EGroupType.None
    )
    {
        foreach (var dto in dtos)
        {
            yield return new Attachment
            {
                Id = dto.Id ?? GuidExtensions.GetSequentialGuidId(),
                ParentId = dto.ParentId,
                GroupId = groupId.ToString(),
                GroupType = groupType.ToString().ResolveSignedGroupType(dto.ParentId != null),
                Type = dto.Type,
                FileName = dto.FileName,
                OriginalName = dto.OriginalName,
                Path = dto.Path,
                Size = dto.Size,
            };
        }
    }

    public static IEnumerable<Attachment> ToEntities( this List<TepDinhKemDto> dtos, Guid groupId, string groupType = "None" )
    {
        foreach (var dto in dtos)
        {
            yield return new Attachment
            {
                Id = dto.Id ?? GuidExtensions.GetSequentialGuidId(),
                ParentId = dto.ParentId,
                GroupId = groupId.ToString(),
                GroupType = groupType.ResolveSignedGroupType(dto.ParentId != null),
                Type = dto.Type,
                FileName = dto.FileName,
                OriginalName = dto.OriginalName,
                Path = dto.Path,
                Size = dto.Size,
            };
        }
    }
}