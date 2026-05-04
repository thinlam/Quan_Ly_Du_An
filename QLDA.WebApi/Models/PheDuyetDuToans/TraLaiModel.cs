namespace QLDA.WebApi.Models.PheDuyetDuToans;

/// <summary>
/// Model cho request trả lại phê duyệt dự toán
/// </summary>
public class TraLaiModel {
    /// <summary>
    /// Lý do trả lại (bắt buộc)
    /// </summary>
    public string NoiDung { get; set; } = string.Empty;
}