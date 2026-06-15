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
}
