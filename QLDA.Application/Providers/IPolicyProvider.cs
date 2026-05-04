using BuildingBlocks.Domain.Providers;

namespace QLDA.Application.Providers;

/// <summary>
/// Policy provider - checks role-permission toggles from CauHinhVaiTroQuyen table
/// </summary>
public interface IPolicyProvider {
    /// <summary>
    /// Check if any of user's roles has a specific permission activated
    /// </summary>
    bool HasPermission(IEnumerable<string> roles, string permissionKey);

    /// <summary>
    /// Get all activated permission keys for given roles
    /// </summary>
    List<string> GetActivePermissions(IEnumerable<string> roles);

    /// <summary>
    /// Check if user can view all entities in a module (has XemTatCa permission)
    /// </summary>
    bool CanViewAll(IUserProvider user, string xemTatCaPermission);

    /// <summary>
    /// Check if user can view department-scoped entities (has XemTheoPhong permission)
    /// </summary>
    bool CanViewByPhong(IUserProvider user, string xemTheoPhongPermission);
}
