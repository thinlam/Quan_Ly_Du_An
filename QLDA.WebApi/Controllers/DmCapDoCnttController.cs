using QLDA.WebApi.Models.DmCapDoCntts;
using QLDA.Domain.Constants;

namespace QLDA.WebApi.Controllers;

[Tags("Danh mục cấp độ CNTT")]
[Route("api/danh-muc-cap-do-cntt")]
[Authorize(Roles = RoleConstants.GroupAdminOrManager)]
public class DmCapDoCnttController(IServiceProvider sp) : AggregateRootController(sp) {

    [HttpGet("{id}")]
    public async Task<ResultApi> Get(int id) {
        var entity = await Mediator.Send(new DanhMucGetQuery {
            DanhMuc = EDanhMuc.DmCapDoCntt,
            Id = id.ToString(),
            ThrowIfNull = true
        }) as DmCapDoCntt;
        return ResultApi.Ok(entity!.ToModel());
    }

    [HttpGet("danh-sach")]
    public async Task<ResultApi> GetAll([FromQuery] AggregateRootPagination req, string? globalFilter) {
        var result = await Mediator.Send(new DanhMucGetDanhSachQuery {
            DanhMuc = EDanhMuc.DmCapDoCntt,
            GlobalFilter = globalFilter,
            PageIndex = req.PageIndex,
            PageSize = req.PageSize
        });
        return ResultApi.Ok(result);
    }

    [HttpGet("combobox")]
    public async Task<ResultApi> GetCombobox(string? globalFilter = null) {
        var entities = await Mediator.Send(new DanhMucGetDanhSachQuery {
            DanhMuc = EDanhMuc.DmCapDoCntt,
            GlobalFilter = globalFilter,
            PageSize = 1000
        }) as PaginatedList<DmCapDoCntt>;
        return ResultApi.Ok(entities?.Data.Select(e => e.ToModel()).ToList());
    }

    [HttpPost("them-moi")]
    public async Task<ResultApi> Create([FromBody] DmCapDoCnttModel model) {
        var entity = model.ToEntity();
        await Mediator.Send(new DanhMucInsertOrUpdateCommand(entity, EDanhMuc.DmCapDoCntt));
        return ResultApi.Ok(entity.ToModel());
    }

    [HttpPut("cap-nhat")]
    public async Task<ResultApi> Update([FromBody] DmCapDoCnttModel model) {
        ManagedException.ThrowIfNull(model.Id, "Id (định danh) không được để trống khi cập nhật");
        var entity = model.ToEntity();
        await Mediator.Send(new DanhMucInsertOrUpdateCommand(entity, EDanhMuc.DmCapDoCntt));
        return ResultApi.Ok(entity.ToModel());
    }

    [HttpDelete("xoa-tam/{id}")]
    public async Task<ResultApi> SoftDelete(int id) {
        var entity = await Mediator.Send(new DanhMucGetQuery {
            Id = id.ToString(),
            DanhMuc = EDanhMuc.DmCapDoCntt,
            ThrowIfNull = true
        }) as DmCapDoCntt;
        entity!.IsDeleted = true;
        await Mediator.Send(new DanhMucInsertOrUpdateCommand(entity, EDanhMuc.DmCapDoCntt));
        return ResultApi.Ok(entity.ToModel());
    }
}