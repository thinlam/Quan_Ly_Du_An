using BuildingBlocks.Application.Attachments.DTOs;

namespace BuildingBlocks.Application.Attachments.Common;

/// <summary>
/// Helper non-generic cho load Attachment.
/// <list type="bullet">
/// <item><b>Standalone</b> (ngoài expression tree): dùng extension ForGroupTypes / ForExactGroupType / …</item>
/// <item><b>Correlated subquery trong .Select()</b>: gọi ExpandGroupTypes trước query,
/// rồi <c>types.Contains(a.GroupType)</c> trong Where — EF Core translate được.</item>
/// </list>
/// Không generic theo parent entity — nhận <c>string groupId</c> trực tiếp.
/// </summary>
public static class AttachmentSubquery
{
    /// <summary>
    /// Mở rộng base GroupTypes → danh sách exact GroupType để filter.
    /// Loại bỏ null/empty/whitespace và duplicate. Không double-prefix KySo_.
    /// </summary>
    public static IReadOnlyList<string> ExpandGroupTypes(
        bool includeSigned,
        params string[] baseGroupTypes)
        => ExpandGroupTypes(baseGroupTypes, includeSigned);

    /// <summary>
    /// Mở rộng base GroupTypes → danh sách exact GroupType để filter.
    /// </summary>
    public static IReadOnlyList<string> ExpandGroupTypes(
        IEnumerable<string>? baseGroupTypes,
        bool includeSigned = true)
    {
        if (baseGroupTypes is null)
            return Array.Empty<string>();

        var result = new List<string>();
        foreach (var raw in baseGroupTypes)
        {
            if (string.IsNullOrWhiteSpace(raw))
                continue;

            var baseType = raw.Trim();
            if (!result.Contains(baseType, StringComparer.Ordinal))
                result.Add(baseType);

            if (!includeSigned)
                continue;

            var signed = SignedGroupTypeHelper.WithSignedVariant(baseType);
            if (!result.Contains(signed, StringComparer.Ordinal))
                result.Add(signed);
        }

        return result;
    }

    /// <summary>
    /// Filter theo nhiều base GroupType. Mặc định gộp cả KySo_ variant.
    /// Trả về <see cref="IQueryable{Attachment}"/> — caller tự <c>Select(ToDto)</c>.
    /// </summary>
    public static IQueryable<Attachment> ForGroupTypes(
        this IQueryable<Attachment> query,
        string groupId,
        bool includeSigned,
        params string[] baseGroupTypes)
    {
        var allTypes = ExpandGroupTypes(baseGroupTypes, includeSigned);
        if (allTypes.Count == 0)
            return query.Where(_ => false);

        return query.Where(a => a.GroupId == groupId && allTypes.Contains(a.GroupType));
    }

    /// <summary>
    /// Filter theo nhiều base GroupType, IncludeSigned = true.
    /// </summary>
    public static IQueryable<Attachment> ForGroupTypes(
        this IQueryable<Attachment> query,
        string groupId,
        params string[] baseGroupTypes)
        => query.ForGroupTypes(groupId, includeSigned: true, baseGroupTypes);

    /// <summary>
    /// Nhận enum trực tiếp, convert sang string bên trong.
    /// </summary>
    public static IQueryable<Attachment> ForGroupTypes<TEnum>(
        this IQueryable<Attachment> query,
        string groupId,
        bool includeSigned,
        params TEnum[] baseGroupTypes)
        where TEnum : struct, Enum
    {
        var baseTypes = baseGroupTypes.Select(g => g.ToString()!).ToArray();
        return query.ForGroupTypes(groupId, includeSigned, baseTypes);
    }

    /// <summary>
    /// Nhận enum, IncludeSigned = true.
    /// </summary>
    public static IQueryable<Attachment> ForGroupTypes<TEnum>(
        this IQueryable<Attachment> query,
        string groupId,
        params TEnum[] baseGroupTypes)
        where TEnum : struct, Enum
        => query.ForGroupTypes(groupId, includeSigned: true, baseGroupTypes);

    /// <summary>
    /// Exact một GroupType — không tự thêm KySo_.
    /// </summary>
    public static IQueryable<Attachment> ForExactGroupType(
        this IQueryable<Attachment> query,
        string groupId,
        string exactGroupType)
    {
        if (string.IsNullOrWhiteSpace(exactGroupType))
            return query.Where(_ => false);

        return query.Where(a => a.GroupId == groupId && a.GroupType == exactGroupType);
    }

    /// <summary>
    /// Exact một GroupType từ enum.
    /// </summary>
    public static IQueryable<Attachment> ForExactGroupType<TEnum>(
        this IQueryable<Attachment> query,
        string groupId,
        TEnum exactGroupType)
        where TEnum : struct, Enum
        => query.ForExactGroupType(groupId, exactGroupType.ToString()!);

    /// <summary>
    /// Tất cả file của 1 GroupId (mọi GroupType).
    /// </summary>
    public static IQueryable<Attachment> ForGroupId(
        this IQueryable<Attachment> query,
        string groupId)
        => query.Where(a => a.GroupId == groupId);

    /// <summary>
    /// Chỉ file gốc (GroupType == base). Không nhận base đã có prefix KySo_.
    /// </summary>
    public static IQueryable<Attachment> OriginalOnly(
        this IQueryable<Attachment> query,
        string groupId,
        string baseGroupType)
    {
        if (string.IsNullOrWhiteSpace(baseGroupType))
            throw new ArgumentException("baseGroupType là bắt buộc.", nameof(baseGroupType));

        if (baseGroupType.IsSignedVariant())
            throw new ArgumentException(
                $"OriginalOnly không nhận GroupType đã có prefix '{SignedGroupTypeHelper.Prefix}'. Nhận: '{baseGroupType}'.",
                nameof(baseGroupType));

        return query.Where(a => a.GroupId == groupId && a.GroupType == baseGroupType);
    }

    /// <summary>
    /// Chỉ file ký số (GroupType == KySo_&lt;base&gt;). Dùng SignedGroupTypeHelper — không double prefix.
    /// </summary>
    public static IQueryable<Attachment> SignedOnly(
        this IQueryable<Attachment> query,
        string groupId,
        string baseGroupType)
    {
        if (string.IsNullOrWhiteSpace(baseGroupType))
            throw new ArgumentException("baseGroupType là bắt buộc.", nameof(baseGroupType));

        var signedType = SignedGroupTypeHelper.WithSignedVariant(baseGroupType);
        return query.Where(a => a.GroupId == groupId && a.GroupType == signedType);
    }

    /// <summary>
    /// Projection sang AttachmentDto — dùng ngoài correlated Select khi không cần module DTO.
    /// </summary>
    public static IQueryable<AttachmentDto> SelectDto(this IQueryable<Attachment> query)
        => query.Select(a => a.ToDto());
}
