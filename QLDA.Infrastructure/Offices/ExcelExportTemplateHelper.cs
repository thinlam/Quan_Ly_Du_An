using Aspose.Cells;
using BuildingBlocks.CrossCutting.Offices;
using BuildingBlocks.Infrastructure.Offices;

namespace QLDA.Infrastructure.Offices;

/// <summary>
/// Applies static placeholder replacements to an export template before row binding.
/// </summary>
public static class ExcelExportTemplateHelper {
    public static string PrepareTemplateWithPlaceholders(
        IAsposeHelper asposeHelper,
        string sourceTemplatePath,
        Dictionary<string, string> replacements) {
        asposeHelper.EnsureCellsLicense();

        var tempPath = Path.Combine(Path.GetTempPath(), $"qlda_export_{Guid.NewGuid():N}.xlsx");
        var workbook = new Workbook(sourceTemplatePath);
        workbook.Worksheets[0].ReplacePlaceholders(replacements);
        workbook.Save(tempPath);
        return tempPath;
    }
}
