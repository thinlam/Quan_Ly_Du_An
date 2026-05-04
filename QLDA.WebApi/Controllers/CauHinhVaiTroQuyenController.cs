using QLDA.Application.CauHinhVaiTroQuyens.Commands;
using QLDA.Application.CauHinhVaiTroQuyens.DTOs;
using QLDA.Application.CauHinhVaiTroQuyens.Queries;
using QLDA.Domain.Constants;

namespace QLDA.WebApi.Controllers;

[Tags("Cấu hình quyền")]
[Route("api/cau-hinh-vai-tro-quyen")]
public class CauHinhVaiTroQuyenController(IServiceProvider serviceProvider) : AggregateRootController(serviceProvider) {

    /// <summary>
    /// Danh sách cấu hình quyền theo vai trò (grouped by NhomQuyen)
    /// </summary>
    [HttpGet("danh-sach")]
    [ProducesResponseType<ResultApi<List<CauHinhVaiTroQuyenDto>>>(StatusCodes.Status200OK)]
    public async Task<ResultApi> GetDanhSach([FromQuery] string? vaiTro, [FromQuery] string? nhomQuyen) {
        var result = await Mediator.Send(new CauHinhVaiTroQuyenGetDanhSachQuery {
            VaiTro = vaiTro,
            NhomQuyen = nhomQuyen,
        });
        return ResultApi.Ok(result);
    }

    /// <summary>
    /// Cập nhật bật/tắt quyền hàng loạt (chỉ Admin/Quản trị)
    /// </summary>
    [Authorize(Roles = $"{RoleConstants.QLDA_TatCa},{RoleConstants.QLDA_QuanTri}")]
    [HttpPut("cap-nhat")]
    [ProducesResponseType<ResultApi<int>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ResultApi>(StatusCodes.Status400BadRequest)]
    public async Task<ResultApi> BatchUpdate([FromBody] CauHinhVaiTroQuyenUpdateDto dto) {
        var count = await Mediator.Send(new CauHinhVaiTroQuyenBatchUpdateCommand(dto));
        return ResultApi.Ok(count);
    }
}
