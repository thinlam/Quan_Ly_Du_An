using System.Data;
using BuildingBlocks.CrossCutting.Offices;
using Microsoft.Extensions.DependencyInjection;

namespace BuildingBlocks.Infrastructure.Offices;

public class WordHelper(IAsposeHelper asposeHelper) : IWordHelper
{
    private readonly IAsposeHelper _asposeHelper = asposeHelper;

    /// <inheritdoc />
    public byte[] ExportFromTemplate(string templatePath, Dictionary<string, string> fieldValues) {
        _asposeHelper.EnsureLicense();

        var doc = new Aspose.Words.Document(templatePath);
        doc.MailMerge.UseNonMergeFields = true;

        foreach (var kvp in fieldValues) {
            doc.MailMerge.Execute(new[] { kvp.Key }, new object[] { kvp.Value });
        }

        using var ms = new MemoryStream();
        doc.Save(ms, Aspose.Words.SaveFormat.Docx);
        return ms.ToArray();
    }
    public byte[] ExportFromTemplate(string templatePath,DataSet tables, Dictionary<string, string> fieldValues) {
        _asposeHelper.EnsureLicense();

        var doc = new Aspose.Words.Document(templatePath);
        doc.MailMerge.UseNonMergeFields = true;

        foreach (var kvp in fieldValues) {
            doc.MailMerge.Execute(new[] { kvp.Key }, new object[] { kvp.Value });
        }
        if (tables != null) {
            if (tables.Tables.Count > 0) {
                foreach (DataTable item in tables.Tables) {
                    var sttColumn = item.Columns
                        .Cast<DataColumn>()
                        .FirstOrDefault(c =>
                            string.Equals(c.ColumnName, "STT", StringComparison.OrdinalIgnoreCase));

                        // Create STT column if missing
                        if (sttColumn == null) {
                            item.Columns.Add("STT", typeof(int));

                            for (int i = 0; i < item.Rows.Count; i++) {
                                item.Rows[i]["STT"] = i + 1;
                            }
                        }
                        doc.MailMerge.ExecuteWithRegions(item);
                }
            }
        }

        using var ms = new MemoryStream();
        doc.Save(ms, Aspose.Words.SaveFormat.Docx);
        return ms.ToArray();

    }
}
