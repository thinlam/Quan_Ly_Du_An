namespace BuildingBlocks.CrossCutting.Offices;

public class AsposeResult
{
    public required byte[] FileBytes { get; set; }
    public required string ContentType { get; set; }
}

public class AsposeInstruction<T>
{
    public required string TemplatePath { get; set; }
    public required List<T> Items { get; set; }
    public List<string> HiddenColumns { get; set; } = [];
    public List<MergeInstruction>? MergeInstructions { get; set; }

    /// <summary>
    /// Khi false: giữ nguyên độ rộng cột / cao dòng đã chỉnh trong template (không gọi AutoFit).
    /// </summary>
    public bool AutoFitColumnsAndRows { get; set; } = true;
}

/// <summary>
/// Instruction for dynamic export using dictionary data without property reflection.
/// Reads column mappings from Excel template ($FieldName markers).
/// </summary>
public class DynamicExportInstruction
{
    public required string TemplatePath { get; set; }
    public required List<Dictionary<string, object?>> Items { get; set; }
    public List<string> HiddenColumns { get; set; } = [];
}