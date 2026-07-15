using System.Net.Mime;
using QLDA.Application.KySos.Commands;
using QLDA.Application.KySos.DTOs;
using QLDA.Application.KySos.Queries;
using QLDA.WebApi.Models.KySos;
using QLDA.WebApi.Models.TepDinhKems;

namespace QLDA.WebApi.Controllers;

[Tags("Quản lý ký số(quan-ly-ky-so)")]
public class QuanLyKySoController(IServiceProvider serviceProvider) : AggregateRootController(serviceProvider) {

    [HttpGet("api/quan-ly-ky-so/{id}/chi-tiet")]
    [ProducesResponseType<ResultApi<KySoDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ResultApi>(StatusCodes.Status400BadRequest)]
    public async Task<ResultApi> Get(Guid id) {
        var entity = await Mediator.Send(new KySoGetQuery { Id = id });
        return ResultApi.Ok(entity.ToDto());
    }

    [HttpGet("api/quan-ly-ky-so/danh-sach")]
    [ProducesResponseType<ResultApi<PaginatedList<KySoDto>>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ResultApi>(StatusCodes.Status400BadRequest)]
    public async Task<ResultApi> GetList([FromQuery] KySoSearchDto searchDto,
        [FromQuery] AggregateRootPagination pagination) {
        var res = await Mediator.Send(new KySoGetDanhSachQuery(searchDto) {
            PageIndex = pagination.PageIndex,
            PageSize = pagination.PageSize,
            GlobalFilter = searchDto.GlobalFilter
        });
        return ResultApi.Ok(res);
    }
    /// <summary>
    ///
    /// </summary>
    /// <remarks>
    /// GroupId là id của dối tượng chính có file ký số - guid
    /// </remarks>
    /// <param name="model"></param>
    [ProducesResponseType<ResultApi<int>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ResultApi>(StatusCodes.Status400BadRequest)]
    [HttpPost("api/quan-ly-ky-so/ky-so")]
    [Consumes(MediaTypeNames.Application.Json)]
    public async Task<ResultApi> Create([FromBody] KySoModel model) {
        ManagedException.ThrowIfNull(model.DanhSachTepDinhKem);
        model.DanhSachTepDinhKem ??= [];

        var entities = model.DanhSachTepDinhKem.ToEntities(model.GroupId, EGroupType.KySo)
            .ToList();

        var count = await Mediator.Send(new NoiDungDaKyCommand {
            GroupId = model.GroupId.ToString(),
            Entities = entities,
        });

        return ResultApi.Ok(count);
    }
    [HttpGet("api/quan-ly-ky-so/noi-dung-da-ky/danh-sach")]
    [ProducesResponseType<ResultApi<PaginatedList<KySoDto>>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ResultApi>(StatusCodes.Status400BadRequest)]
    public async Task<ResultApi> GetSigned([FromQuery] NoiDungDaKySearchDto searchDto, [FromQuery] AggregateRootPagination pagination) {
        var res = await Mediator.Send(new NoiDungDaKyGetDanhSachQuery(searchDto) {
            PageIndex = pagination.PageIndex,
            PageSize = pagination.PageSize,
        });
        return ResultApi.Ok(res);
    }
    [HttpPost("api/quan-ly-ky-so/them-moi")]
    [Consumes(MediaTypeNames.Application.Json)]
    [ProducesResponseType<ResultApi<KySoDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ResultApi>(StatusCodes.Status400BadRequest)]
    public async Task<ResultApi> Insert([FromBody] KySoInsertDto dto) {
        var entity = await Mediator.Send(new KySoInsertCommand(dto));
        return ResultApi.Ok(entity.ToDto());
    }

    [HttpPut("api/quan-ly-ky-so/cap-nhat")]
    [Consumes(MediaTypeNames.Application.Json)]
    [ProducesResponseType<ResultApi<KySoDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ResultApi>(StatusCodes.Status400BadRequest)]
    public async Task<ResultApi> Update([FromBody] KySoUpdateModel model) {
        var entity = await Mediator.Send(new KySoUpdateCommand(model));
        return ResultApi.Ok(entity.ToDto());
    }

    [HttpDelete("api/quan-ly-ky-so/{id}/xoa-tam")]
    [ProducesResponseType<ResultApi<int>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ResultApi>(StatusCodes.Status400BadRequest)]
    public async Task<ResultApi> SoftDelete(Guid id) {
        await Mediator.Send(new KySoDeleteCommand(id));
        return ResultApi.Ok(1);
    }
}
