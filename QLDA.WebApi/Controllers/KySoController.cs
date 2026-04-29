using System.Net.Mime;
using QLDA.Application.KySos.Commands;
using QLDA.Application.KySos.DTOs;
using QLDA.Application.KySos.Queries;
using QLDA.Application.TepDinhKems.Commands;
using QLDA.Domain.Constants;
using QLDA.WebApi.Models.KySos;
using QLDA.WebApi.Models.TepDinhKems;

namespace QLDA.WebApi.Controllers;

/// <summary>
/// Lưu file ký s06ac6528-df5a-f011-a9bf-0050568a8a95
/// </summary>
/// <param name="serviceProvider"></param>
[Tags("Ký số")]
[Route("api/ky-so")]
public class KySoController(IServiceProvider serviceProvider) : AggregateRootController(serviceProvider) {
    /// <summary>
    /// 
    /// </summary>
    /// <remarks>
    /// GroupId là id của dối tượng chính có file ký số - guid
    /// </remarks>
    /// <param name="model"></param>
    [ProducesResponseType<ResultApi<int>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ResultApi>(StatusCodes.Status400BadRequest)]
    [HttpPost("them-moi")]
    [Consumes(MediaTypeNames.Application.Json)]
    public async Task<ResultApi> Create([FromBody] KySoModel model) {
        ManagedException.ThrowIfNull(model.DanhSachTepDinhKem);
        ManagedException.ThrowIfNull(model.DanhSachTepDinhKem.Any(e => e.ParentId == null));
        model.DanhSachTepDinhKem ??= [];
        
        await Mediator.Send(new TepDinhKemBulkInsertOrUpdateCommand {
            KySo = true,
            GroupId = model.GroupId.ToString(),
            Entities = [.. model.DanhSachTepDinhKem.ToEntities(model.GroupId, GroupTypeConstants.KySo)]
        });
        return ResultApi.Ok(1);
    }

    [HttpGet("{id}/chi-tiet")]
    [ProducesResponseType<ResultApi<KySoDto>>(StatusCodes.Status200OK)]
    public async Task<ResultApi> Get(Guid id) {
        var entity = await Mediator.Send(new KySoGetQuery { Id = id });
        return ResultApi.Ok(entity.ToDto());
    }

    [HttpGet("danh-sach")]
    [ProducesResponseType<ResultApi<PaginatedList<KySoDto>>>(StatusCodes.Status200OK)]
    public async Task<ResultApi> GetList([FromQuery] KySoSearchDto searchDto, 
        [FromQuery] AggregateRootPagination pagination) {
        var res = await Mediator.Send(new KySoGetDanhSachQuery(searchDto) {
            PageIndex = pagination.PageIndex,
            PageSize = pagination.PageSize,
            GlobalFilter = searchDto.GlobalFilter
        });
        return ResultApi.Ok(res);
    }

    [HttpPost("them-moi-ky-so")]
    [Consumes(MediaTypeNames.Application.Json)]
    [ProducesResponseType<ResultApi<KySoDto>>(StatusCodes.Status200OK)]
    public async Task<ResultApi> Insert([FromBody] KySoInsertDto dto) {
        var entity = await Mediator.Send(new KySoInsertCommand(dto));
        return ResultApi.Ok(entity.ToDto());
    }

    [HttpPut("cap-nhat")]
    [Consumes(MediaTypeNames.Application.Json)]
    [ProducesResponseType<ResultApi<KySoDto>>(StatusCodes.Status200OK)]
    public async Task<ResultApi> Update([FromBody] KySoUpdateModel model) {
        var entity = await Mediator.Send(new KySoUpdateCommand(model));
        return ResultApi.Ok(entity.ToDto());
    }

    [HttpDelete("{id}/xoa-tam")]
    [ProducesResponseType<ResultApi<int>>(StatusCodes.Status200OK)]
    public async Task<ResultApi> SoftDelete(Guid id) {
        await Mediator.Send(new KySoDeleteCommand(id));
        return ResultApi.Ok(1);
    }
}