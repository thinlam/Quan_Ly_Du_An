namespace BuildingBlocks.Application.Attachments.Common;

/// <summary>
/// Single source of truth cho convention ký số trên GroupType.
/// ParentId == null → baseGroupType; ParentId != null → KySo_&lt;baseGroupType&gt;.
/// Không tạo double prefix KySo_KySo_.
/// </summary>
public static class SignedGroupTypeHelper
{
    public const string Prefix = "KySo_";

    /// <summary>
    /// Resolve GroupType theo ParentId / isChild.
    /// Không double-prefix nếu base đã bắt đầu bằng KySo_.
    /// </summary>
    public static string ResolveSignedGroupType(this string baseGroupType, bool isChild)
    {
        if (string.IsNullOrEmpty(baseGroupType))
            return baseGroupType ?? string.Empty;

        return isChild && !baseGroupType.StartsWith(Prefix, StringComparison.Ordinal)
            ? Prefix + baseGroupType
            : baseGroupType;
    }

    /// <summary>
    /// Strip prefix KySo_ để lấy base GroupType. "KySo_QLHD" → "QLHD"; "QLHD" → "QLHD".
    /// </summary>
    public static string? ToBaseGroupType(this string? groupType)
        => groupType?.StartsWith(Prefix, StringComparison.Ordinal) == true
            ? groupType[Prefix.Length..]
            : groupType;

    public static bool IsSignedVariant(this string? groupType)
        => groupType?.StartsWith(Prefix, StringComparison.Ordinal) == true;

    /// <summary>
    /// Trả về biến thể ký số: "QLHD" → "KySo_QLHD". Không double-prefix nếu đã có KySo_.
    /// </summary>
    public static string WithSignedVariant(string baseGroupType)
        => string.IsNullOrEmpty(baseGroupType)
            ? baseGroupType
            : baseGroupType.StartsWith(Prefix, StringComparison.Ordinal)
                ? baseGroupType
                : Prefix + baseGroupType;

    /// <summary>
    /// Mở rộng [base, KySo_base] — dùng khi filter scope sync/load.
    /// Không double-prefix nếu base đã có KySo_.
    /// </summary>
    public static string[] ExpandWithSignedVariant(string baseGroupType)
    {
        if (string.IsNullOrEmpty(baseGroupType))
            return [baseGroupType];

        var signed = WithSignedVariant(baseGroupType);
        return signed == baseGroupType
            ? [baseGroupType]
            : [baseGroupType, signed];
    }

    /// <summary>
    /// Mở rộng nhiều base GroupType (delegate sang AttachmentSubquery.ExpandGroupTypes).
    /// </summary>
    public static IReadOnlyList<string> ExpandGroupTypes(
        IEnumerable<string>? baseGroupTypes,
        bool includeSigned = true)
        => AttachmentSubquery.ExpandGroupTypes(baseGroupTypes, includeSigned);

    /// <summary>
    /// Filter IQueryable theo GroupId + (base OR KySo_base).
    /// </summary>
    public static IQueryable<Attachment> WhereSignedScope(
        this IQueryable<Attachment> query,
        string groupId,
        string baseGroupType)
    {
        var signed = WithSignedVariant(baseGroupType);
        return query.Where(t =>
            t.GroupId == groupId
            && (t.GroupType == baseGroupType || t.GroupType == signed));
    }
}
