namespace QLDA.Application.Common;

public static class SignedHelper {
    public const string Prefix = "KySo_";

    public static string ResolveSignedGroupType(this string baseGroupType, bool isChild)
        => isChild && !baseGroupType.StartsWith(Prefix, StringComparison.Ordinal)
            ? Prefix + baseGroupType
            : baseGroupType;

    public static string? ToBaseGroupType(this string groupType)
        => groupType?.StartsWith(Prefix, StringComparison.Ordinal) == true
            ? groupType[Prefix.Length..]
            : groupType;

    public static bool IsSignedVariant(this string groupType)
        => groupType?.StartsWith(Prefix, StringComparison.Ordinal) == true;

    public static string[] WithSignedVariant(this string baseGroupType)
        => [baseGroupType, Prefix + baseGroupType];

    public static IQueryable<Attachment> WhereSignedScope(
        this IQueryable<Attachment> q,
        string groupId,
        string baseGroupType)
        => q.Where(t =>
            t.GroupId == groupId
            && (t.GroupType == baseGroupType || t.GroupType == Prefix + baseGroupType));
}
