using BuildingBlocks.Application.Attachments.Common;
using BuildingBlocks.Application.Attachments.DTOs;
using BuildingBlocks.Domain.Entities;
using QLDA.Domain.Enums;

namespace QLDA.Application.TepDinhKems.DTOs;

/// <summary>
/// Compatibility mapping — resolve ký số qua SignedGroupTypeHelper (BB Phase 2).
/// </summary>
public static class TepDinhKemMappingConfiguration
{
    public static List<TepDinhKemDto> ToDtos(this IEnumerable<Attachment> danhSachTepDinhKem)
        => [.. danhSachTepDinhKem.Select(o => o.ToDto<TepDinhKemDto>())];

    public static TepDinhKemDto ToDto(this Attachment entity)
        => entity.ToDto<TepDinhKemDto>();

    private static Attachment ToEntity(this TepDinhKemInsertDto insertDto, Guid groupId, EGroupType groupType = EGroupType.None)
        => new()
        {
            Id = GuidExtensions.GetSequentialGuidId(),
            ParentId = insertDto.ParentId,
            GroupId = groupId.ToString(),
            GroupType = SignedGroupTypeHelper.ResolveSignedGroupType(
                groupType.ToString(), insertDto.ParentId != null),
            Type = insertDto.Type,
            FileName = insertDto.FileName,
            OriginalName = insertDto.OriginalName,
            Path = insertDto.Path,
            Size = insertDto.Size,
        };

    private static Attachment ToEntity(this TepDinhKemInsertDto insertDto, Guid groupId, string groupType = "None")
        => new()
        {
            Id = GuidExtensions.GetSequentialGuidId(),
            ParentId = insertDto.ParentId,
            GroupId = groupId.ToString(),
            GroupType = SignedGroupTypeHelper.ResolveSignedGroupType(
                groupType, insertDto.ParentId != null),
            Type = insertDto.Type,
            FileName = insertDto.FileName,
            OriginalName = insertDto.OriginalName,
            Path = insertDto.Path,
            Size = insertDto.Size,
        };

    private static Attachment ToEntity(this TepDinhKemInsertOrUpdateDto insertOrUpdateDto, Guid groupId, EGroupType groupType = EGroupType.None)
        => new()
        {
            Id = insertOrUpdateDto.Id.GetId(),
            ParentId = insertOrUpdateDto.ParentId,
            GroupId = groupId.ToString(),
            GroupType = SignedGroupTypeHelper.ResolveSignedGroupType(
                groupType.ToString(), insertOrUpdateDto.ParentId != null),
            Type = insertOrUpdateDto.Type,
            FileName = insertOrUpdateDto.FileName,
            OriginalName = insertOrUpdateDto.OriginalName,
            Path = insertOrUpdateDto.Path,
            Size = insertOrUpdateDto.Size,
        };

    private static Attachment ToEntity(this TepDinhKemInsertOrUpdateDto insertOrUpdateDto, Guid groupId, string groupType = "None")
        => new()
        {
            Id = insertOrUpdateDto.Id.GetId(),
            ParentId = insertOrUpdateDto.ParentId,
            GroupId = groupId.ToString(),
            GroupType = SignedGroupTypeHelper.ResolveSignedGroupType(
                groupType, insertOrUpdateDto.ParentId != null),
            Type = insertOrUpdateDto.Type,
            FileName = insertOrUpdateDto.FileName,
            OriginalName = insertOrUpdateDto.OriginalName,
            Path = insertOrUpdateDto.Path,
            Size = insertOrUpdateDto.Size,
        };

    public static IEnumerable<Attachment> ToEntities(
        this List<TepDinhKemInsertDto> dtos,
        Guid groupId,
        EGroupType groupType = EGroupType.None)
        => dtos.Select(m => ToEntity(m, groupId, groupType));

    public static IEnumerable<Attachment> ToEntities(
        this List<TepDinhKemInsertDto> dtos,
        Guid groupId,
        string groupType = "None")
        => dtos.Select(m => ToEntity(m, groupId, groupType));

    public static IEnumerable<Attachment> ToEntities(
        this List<TepDinhKemInsertOrUpdateDto> dtos,
        Guid groupId,
        EGroupType groupType = EGroupType.None)
        => dtos.Select(m => ToEntity(m, groupId, groupType));

    public static IEnumerable<Attachment> ToEntities(
        this List<TepDinhKemInsertOrUpdateDto> dtos,
        Guid groupId,
        string groupType = "None")
        => dtos.Select(m => ToEntity(m, groupId, groupType));

    public static IEnumerable<Attachment> ToEntities(
        this List<TepDinhKemDto> dtos,
        Guid groupId,
        EGroupType groupType = EGroupType.None)
        => ((IEnumerable<IAttachmentDto>)dtos).ToEntities(groupId, groupType.ToString());

    public static IEnumerable<Attachment> ToEntities(
        this List<TepDinhKemDto> dtos,
        Guid groupId,
        string groupType = "None")
        => ((IEnumerable<IAttachmentDto>)dtos).ToEntities(groupId, groupType);
}
