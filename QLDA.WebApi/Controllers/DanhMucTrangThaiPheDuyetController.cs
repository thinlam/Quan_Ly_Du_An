using System.Net.Mime;
using QLDA.Domain.Constants;
using QLDA.WebApi.Models.DmTrangThaiPheDuyet;

namespace QLDA.WebApi.Controllers;

[Tags("Danh mục trạng thái phê duyệt")]
[Route("api/danh-muc-trang-thai-phe-duyet")]
[Authorize(Roles = RoleConstants.GroupAdminOrManager)]
public class DanhMucTrangThaiPheDuyetController(IServiceProvider serviceProvider) : AggregateRootController(serviceProvider) {
    [ProducesResponseType<ResultApi<DanhMucTrangThaiPheDuyetModel>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ResultApi>(StatusCodes.Status400BadRequest)]
    [HttpGet("{id}")]
    public async Task<ResultApi> Get(int id) {
        var entity = await Mediator.Send(new DanhMucGetQuery { Id = id.ToString(), DanhMuc = EDanhMuc.DanhMucTrangThaiPheDuyet, ThrowIfNull = true }) as DanhMucTrangThaiPheDuyet;
        var model = entity!.ToModel();
        return ResultApi.Ok(model);
    }

    [ProducesResponseType<ResultApi<DanhMucTrangThaiPheDuyet>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ResultApi>(StatusCodes.Status400BadRequest)]
    [HttpDelete("xoa-tam")]
    public async Task<ResultApi> SoftDelete(int id) {
        var entity = await Mediator.Send(new DanhMucGetQuery { Id = id.ToString(), DanhMuc = EDanhMuc.DanhMucTrangThaiPheDuyet, ThrowIfNull = true }) as DanhMucTrangThaiPheDuyet;
        entity!.IsDeleted = true;
        await Mediator.Send(new DanhMucInsertOrUpdateCommand(entity, EDanhMuc.DanhMucTrangThaiPheDuyet));
        return ResultApi.Ok(entity);
    }

    [ProducesResponseType<ResultApi<PaginatedList<DanhMucDto<int>>>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ResultApi>(StatusCodes.Status400BadRequest)]
    [HttpGet("danh-sach-day-du")]
    public async Task<ResultApi> GetAll(
        [FromQuery] AggregateRootPagination req,
        string? globalFilter,
        string? loai) {
        var res = await Mediator.Send(new DanhMucGetDanhSachQuery() {
            DanhMuc = EDanhMuc.DanhMucTrangThaiPheDuyet,
            PageIndex = req.PageIndex,
            GlobalFilter = globalFilter,
            PageSize = req.PageSize,
            GetAll = true,
            Loai = loai
        });
        return ResultApi.Ok(res);
    }

    [ProducesResponseType<ResultApi<List<DanhMucDto<int>>>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ResultApi>(StatusCodes.Status400BadRequest)]
    [HttpGet("danh-sach")]
    public async Task<ResultApi> Get([FromQuery] List<long>? ids = null, bool getAll = false) {
        var res = await Mediator.Send(new DanhMucGetDanhSachQuery() {
            DanhMuc = EDanhMuc.DanhMucTrangThaiPheDuyet,
            PageIndex = 0,
            PageSize = 0,
            Ids = ids,
            GetAll = getAll,
        }) as PaginatedList<DanhMucDto<int>>;
        return ResultApi.Ok(res == null ? [] : res.Data);
    }

    [ProducesResponseType<ResultApi<int>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ResultApi>(StatusCodes.Status400BadRequest)]
    [HttpPost("them-moi")]
    [Consumes(MediaTypeNames.Application.Json)]
    public async Task<ResultApi> Create([FromBody] DanhMucTrangThaiPheDuyetModel model) {
        var entity = model.ToEntity();
        await Mediator.Send(new DanhMucInsertOrUpdateCommand(entity, EDanhMuc.DanhMucTrangThaiPheDuyet));
        return ResultApi.Ok(1);
    }

    [ProducesResponseType<ResultApi<DanhMucTrangThaiPheDuyetModel>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ResultApi>(StatusCodes.Status400BadRequest)]
    [HttpPut("cap-nhat")]
    [Consumes(MediaTypeNames.Application.Json)]
    public async Task<ResultApi> Update([FromBody] DanhMucTrangThaiPheDuyetModel model) {
        var entity = await Mediator.Send(new DanhMucGetQuery { Id = model.GetId().ToString(), DanhMuc = EDanhMuc.DanhMucTrangThaiPheDuyet, ThrowIfNull = true }) as DanhMucTrangThaiPheDuyet;

        entity!.Ma = model.Ma;
        entity.Ten = model.Ten;
        entity.MoTa = model.MoTa;
        entity.Stt = model.Stt;
        entity.Used = model.Used;
        entity.Loai = model.Loai;

        await Mediator.Send(new DanhMucInsertOrUpdateCommand(entity, EDanhMuc.DanhMucTrangThaiPheDuyet));

        return ResultApi.Ok(model);
    }
}
