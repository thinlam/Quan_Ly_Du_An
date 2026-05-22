using QLDA.Domain.Constants;

namespace QLDA.WebApi.Controllers;

[Tags("System")]
public class SystemController(IServiceProvider serviceProvider) : AggregateRootController(serviceProvider) {
    public static class SystemEndpoints {
        public const string Export = "export/api/{entity}/danh-sach";

        public static readonly Dictionary<string, string> All = new() {
            [Export] = "GET - Export dữ liệu theo entity"
        };
    }

    /// <summary>
    /// Lấy danh sách tất cả endpoints của hệ thống
    /// </summary>
    [HttpGet("api/system/endpoints")]
    public IActionResult GetSystemEndpoints() => Ok(SystemEndpoints.All);

    /// <summary>
    /// Lấy danh sách các entity có thể export
    /// </summary>
    [HttpGet("api/system/entities")]
    public IActionResult GetExportEntities() => Ok(SystemEntityConstants.All);
}