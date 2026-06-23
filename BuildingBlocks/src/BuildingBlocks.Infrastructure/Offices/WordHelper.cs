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
}
