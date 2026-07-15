using System.Data;

namespace BuildingBlocks.CrossCutting.Offices;

/// <summary>
/// Helper cho export Word (.docx) bằng Aspose.Words
/// </summary>
public interface IWordHelper {
    /// <summary>
    /// Export template Word (.docx) bằng MailMerge, thay thế placeholder bằng data.
    /// Template dùng plain text placeholder dạng &lt;key&gt; (không phải Word merge field).
    /// </summary>
    /// <param name="templatePath">Đường dẫn tuyệt đối đến file .docx template</param>
    /// <param name="tables">DataSet chứa các bảng dữ liệu cho MailMerge</param>
    /// <param name="fieldValues">Dictionary placeholder name → giá trị thay thế</param>
    /// <returns>Byte array của file .docx đã fill data</returns>
    byte[] ExportFromTemplate(string templatePath, DataSet tables, Dictionary<string, string> fieldValues);
    byte[] ExportFromTemplate(string templatePath, Dictionary<string, string> fieldValues);
}
