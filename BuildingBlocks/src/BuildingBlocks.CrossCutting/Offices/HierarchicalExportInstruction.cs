namespace BuildingBlocks.CrossCutting.Offices;

/// <summary>
/// Configuration for hierarchical Excel export with grouped headers
/// Template markers:
/// - $Header_{GroupLevel}_{FieldName} for group headers (e.g., $Header_1_LinhVuc, $Header_2_PhongBan)
/// - $Count_{FieldName} for count columns (e.g., $Count_SoLuongNhiemVu)
/// - $Data_{FieldName} for data columns (e.g., $Data_STT, $Data_NoiDung)
/// </summary>
public class HierarchicalExportInstruction<TGroup, TItem> {
    public required string TemplatePath { get; set; }
    public required List<TGroup> Groups { get; set; }

    /// <summary>
    /// Function to get group header text
    /// </summary>
    public required Func<TGroup, string> GetGroupHeader { get; set; }

    /// <summary>
    /// Function to get sub-groups from a group
    /// </summary>
    public Func<TGroup, IEnumerable<object>>? GetSubGroups { get; set; }

    /// <summary>
    /// Function to get sub-group header text
    /// </summary>
    public Func<object, string>? GetSubGroupHeader { get; set; }

    /// <summary>
    /// Function to get items from a sub-group
    /// </summary>
    public Func<object, IEnumerable<TItem>>? GetItems { get; set; }

    /// <summary>
    /// Function to get count for sub-group
    /// </summary>
    public Func<object, int>? GetSubGroupCount { get; set; }

    /// <summary>
    /// Enable Excel row outline grouping (expand/collapse feature)
    /// </summary>
    public bool EnableOutline { get; set; } = false;

    /// <summary>
    /// Collapse groups by default (only applies when EnableOutline = true)
    /// </summary>
    public bool CollapseGroups { get; set; } = false;
}

/// <summary>
/// Simpler instruction for 2-level hierarchy (Group -> Items)
/// </summary>
public class TwoLevelHierarchicalInstruction<TGroup, TSubGroup, TItem> {
    public required string TemplatePath { get; set; }
    public Dictionary<string,string>? PlaceholderReplacements { get; set; }
    public required List<TGroup> Groups { get; set; }

    public required Func<TGroup, string> GroupHeaderFormatter { get; set; }
    public required Func<TGroup, IEnumerable<TSubGroup>> GetSubGroups { get; set; }
    public required Func<TSubGroup, string> SubGroupHeaderFormatter { get; set; }
    public required Func<TSubGroup, int> SubGroupCountGetter { get; set; }
    public required Func<TSubGroup, IEnumerable<TItem>> GetItems { get; set; }
}