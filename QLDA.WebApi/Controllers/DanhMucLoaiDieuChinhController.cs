using System.Net.Mime;
using QLDA.Application.DanhMucLoaiDieuChinhs.Commands;
using QLDA.Application.DanhMucLoaiDieuChinhs.DTOs;
using QLDA.Application.DanhMucLoaiDieuChinhs.Queries;
using QLDA.Domain.Constants;

namespace QLDA.WebApi.Controllers;

[Tags("Danh mục loại điều chỉnh")]
[Route("api/danh-muc-loai-dieu-chinh")]
[Authorize(Roles = RoleConstants.GroupAdminOrManager)]
public class DanhMucLoaiDieuChinhController : AggregateRootController {
    public DanhMucLoaiDieuChinhController(IServiceProvider serviceProvider) : base(serviceProvider) {
    }

    [ProducesResponseType<ResultApi<DanhMucLoaiDieuChinhDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ResultApi>(StatusCodes.Status400BadRequest)]
    [HttpGet("{id}")]
    public async Task<ResultApi> Get(int id) {
        var dto = await Mediator.Send(new DanhMucLoaiDieuChinhGetByIdQuery(id));
        if (dto == null) {
            return ResultApi.Fail("Không tìm thấy dữ liệu");
        }
        return ResultApi.Ok(dto);
    }

    [ProducesResponseType<ResultApi>(StatusCodes.Status200OK)]
    [ProducesResponseType<ResultApi>(StatusCodes.Status400BadRequest)]
    [HttpDelete("xoa-tam/{id}")]
    public async Task<ResultApi> SoftDelete(int id) {
        await Mediator.Send(new DanhMucLoaiDieuChinhDeleteCommand(id));
        return ResultApi.Ok(true);
    }

    [ProducesResponseType<ResultApi<PaginatedList<DanhMucLoaiDieuChinhDto>>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ResultApi>(StatusCodes.Status400BadRequest)]
    [HttpGet("danh-sach-day-du")]
    public async Task<ResultApi> GetAll([FromQuery] AggregateRootPagination req, string? globalFilter) {
        var res = await Mediator.Send(new DanhMucLoaiDieuChinhGetDanhSachQuery {
            GetAll = true
        });
        return ResultApi.Ok(res);
    }

    [ProducesResponseType<ResultApi<List<DanhMucLoaiDieuChinhDto>>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ResultApi>(StatusCodes.Status400BadRequest)]
    [HttpGet("danh-sach")]
    public async Task<ResultApi> Get([FromQuery] List<long>? ids = null, bool getAll = false) {
        var res = await Mediator.Send(new DanhMucLoaiDieuChinhGetDanhSachQuery {
            GetAll = getAll,
        });
        return ResultApi.Ok(res.Data);
    }

    [ProducesResponseType<ResultApi<DanhMucLoaiDieuChinhDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ResultApi>(StatusCodes.Status400BadRequest)]
    [HttpPost("them-moi")]
    [Consumes(MediaTypeNames.Application.Json)]
    public async Task<ResultApi> Create([FromBody] DanhMucLoaiDieuChinhInsertDto dto) {
        var result = await Mediator.Send(new DanhMucLoaiDieuChinhInsertCommand(dto));
        return ResultApi.Ok(result);
    }

    [ProducesResponseType<ResultApi<DanhMucLoaiDieuChinhDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ResultApi>(StatusCodes.Status400BadRequest)]
    [HttpPut("cap-nhat")]
    [Consumes(MediaTypeNames.Application.Json)]
    public async Task<ResultApi> Update([FromBody] DanhMucLoaiDieuChinhUpdateDto dto) {
        var result = await Mediator.Send(new DanhMucLoaiDieuChinhUpdateCommand(dto));
        return ResultApi.Ok(result);
    }
}