namespace QLDA.Application.Authorization;

/// <summary>
/// Constants for resource keys used across authorization providers.
/// Must be used when registering providers to avoid string mismatch.
/// </summary>
public static class AuthorizationResourceKeys
{
    public const string DuAn = "DuAn";
    public const string DuAnBuoc = "DuAnBuoc";
    public const string HopDong = "HopDong";
    public const string GoiThau = "GoiThau";
    public const string VanBan = "VanBan";

    /// <summary>
    /// All defined resource keys. Used by startup registration to wire up providers
    /// without each registration site needing to know every key.
    /// </summary>
    public static readonly string[] All = [DuAn, DuAnBuoc, HopDong, GoiThau, VanBan];
}
