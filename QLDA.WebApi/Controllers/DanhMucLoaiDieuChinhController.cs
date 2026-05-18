using System.Net.Mime;
using QLDA.Domain.Constants;
using QLDA.WebApi.Models.DmLoaiDieuChinhs;

namespace QLDA.WebApi.Controllers;

[Tags("Danh mục loại điều chỉnh")]
[Route("api/danh-muc-loai-dieu-chinh")]
[Authorize(Roles = RoleConstants.GroupAdminOrManager)]
public class DanhMucLoaiDieuChinhController : AggregateRootController {
    public DanhMucLoaiDieuChinhController(IServiceProvider serviceProvider) : base(serviceProvider) {
    }

    [ProducesResponseType<ResultApi<DanhMucLoaiDieuChinhModel>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ResultApi>(StatusCodes.Status400BadRequest)]
    [HttpGet("{id}")]
    public async Task<ResultApi> Get(int id) {
        var entity = await Mediator.Send(new DanhMucGetQuery {
            Id = id.ToString(),
            DanhMuc = EDanhMuc.DanhMucLoaiDieuChinh,
            ThrowIfNull = true,
        }) as DanhMucLoaiDieuChinh;
        var model = entity!.ToModel();
        return ResultApi.Ok(model);
    }

    /// <summary>
    /// Tạm ẩn
    /// </summary>
    [ProducesResponseType<ResultApi<DanhMucLoaiDieuChinh>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ResultApi>(StatusCodes.Status400BadRequest)]
    [HttpDelete("xoa-tam")]
    public async Task<ResultApi> SoftDelete(int id) {
        var entity = await Mediator.Send(new DanhMucGetQuery {
            Id = id.ToString(),
            DanhMuc = EDanhMuc.DanhMucLoaiDieuChinh,
            ThrowIfNull = true,
        }) as DanhMucLoaiDieuChinh;
        entity!.IsDeleted = true;
        await Mediator.Send(new DanhMucInsertOrUpdateCommand(entity, EDanhMuc.DanhMucLoaiDieuChinh));
        return ResultApi.Ok(entity);
    }

    [ProducesResponseType<ResultApi<PaginatedList<DanhMucDto<int>>>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ResultApi>(StatusCodes.Status400BadRequest)]
    [HttpGet("danh-sach-day-du")]
    public async Task<ResultApi> GetAll([FromQuery] AggregateRootPagination req, string? globalFilter) {
        var res = await Mediator.Send(new DanhMucGetDanhSachQuery {
            DanhMuc = EDanhMuc.DanhMucLoaiDieuChinh,
            PageIndex = req.PageIndex,
            GlobalFilter = globalFilter,
            PageSize = req.PageSize,
            GetAll = true
        });
        return ResultApi.Ok(res);
    }

    [ProducesResponseType<ResultApi<List<DanhMucDto<int>>>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ResultApi>(StatusCodes.Status400BadRequest)]
    [HttpGet("danh-sach")]
    public async Task<ResultApi> Get([FromQuery] List<long>? ids = null, bool getAll = false) {
        var res = await Mediator.Send(new DanhMucGetDanhSachQuery {
            DanhMuc = EDanhMuc.DanhMucLoaiDieuChinh,
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
    public async Task<ResultApi> Create([FromBody] DanhMucLoaiDieuChinhModel model) {
        var entity = model.ToEntity();
        await Mediator.Send(new DanhMucInsertOrUpdateCommand(entity, EDanhMuc.DanhMucLoaiDieuChinh));
        return ResultApi.Ok(1);
    }

    [ProducesResponseType<ResultApi<DanhMucLoaiDieuChinhModel>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ResultApi>(StatusCodes.Status400BadRequest)]
    [HttpPut("cap-nhat")]
    [Consumes(MediaTypeNames.Application.Json)]
    public async Task<ResultApi> Update([FromBody] DanhMucLoaiDieuChinhModel model) {
        var entity = await Mediator.Send(new DanhMucGetQuery {
            Id = model.GetId().ToString(),
            DanhMuc = EDanhMuc.DanhMucLoaiDieuChinh,
            ThrowIfNull = true,
        }) as DanhMucLoaiDieuChinh;

        entity!.Ma = model.Ma;
        entity.Ten = model.Ten;
        entity.MoTa = model.MoTa;
        entity.Stt = model.Stt;
        entity.Used = model.Used;

        await Mediator.Send(new DanhMucInsertOrUpdateCommand(entity, EDanhMuc.DanhMucLoaiDieuChinh));

        return ResultApi.Ok(model);
    }
}