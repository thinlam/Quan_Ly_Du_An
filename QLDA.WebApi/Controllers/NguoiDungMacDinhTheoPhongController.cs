using System.Net.Mime;
using QLDA.Application.NguoiDungMacDinhTheoPhongs.Commands;
using QLDA.Application.NguoiDungMacDinhTheoPhongs.DTOs;
using QLDA.Application.NguoiDungMacDinhTheoPhongs.Queries;
using QLDA.Domain.Constants;

namespace QLDA.WebApi.Controllers;

[Tags("Người dùng mặc định theo phòng ban")]
[Route("api/nguoi-dung-mac-dinh-theo-phong")]
[Authorize(Roles = RoleConstants.GroupAdminOrManager)]
public class NguoiDungMacDinhTheoPhongController(IServiceProvider serviceProvider)
    : AggregateRootController(serviceProvider)
{
    [ProducesResponseType<ResultApi<PaginatedList<NguoiDungMacDinhTheoPhongDto>>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ResultApi>(StatusCodes.Status400BadRequest)]
    [HttpGet("danh-sach")]
    public async Task<ResultApi> GetDanhSach([FromQuery] NguoiDungMacDinhTheoPhongSearchDto searchDto)
    {
        var res = await Mediator.Send(new NguoiDungMacDinhTheoPhongGetDanhSachQuery(searchDto));
        return ResultApi.Ok(res);
    }

    [ProducesResponseType<ResultApi<NguoiDungMacDinhTheoPhongDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ResultApi>(StatusCodes.Status400BadRequest)]
    [HttpGet("{id:guid}")]
    public async Task<ResultApi> GetById(Guid id)
    {
        var dto = await Mediator.Send(new NguoiDungMacDinhTheoPhongGetByIdQuery(id));
        return ResultApi.Ok(dto);
    }

    [ProducesResponseType<ResultApi<NguoiDungMacDinhTheoPhongDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ResultApi>(StatusCodes.Status400BadRequest)]
    [HttpPost("them-moi")]
    [Consumes(MediaTypeNames.Application.Json)]
    public async Task<ResultApi> Create([FromBody] NguoiDungMacDinhTheoPhongCreateDto dto)
    {
        var result = await Mediator.Send(new NguoiDungMacDinhTheoPhongInsertCommand(dto));
        return ResultApi.Ok(result);
    }

    [ProducesResponseType<ResultApi<NguoiDungMacDinhTheoPhongDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ResultApi>(StatusCodes.Status400BadRequest)]
    [HttpPut("cap-nhat/{id:guid}")]
    [Consumes(MediaTypeNames.Application.Json)]
    public async Task<ResultApi> Update(Guid id, [FromBody] NguoiDungMacDinhTheoPhongUpdateDto dto)
    {
        ManagedException.ThrowIf(id != dto.Id, "Id không khớp");
        var result = await Mediator.Send(new NguoiDungMacDinhTheoPhongUpdateCommand(dto));
        return ResultApi.Ok(result);
    }

    [ProducesResponseType<ResultApi<bool>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ResultApi>(StatusCodes.Status400BadRequest)]
    [HttpDelete("xoa/{id:guid}")]
    public async Task<ResultApi> Delete(Guid id)
    {
        await Mediator.Send(new NguoiDungMacDinhTheoPhongDeleteCommand(id));
        return ResultApi.Ok(true);
    }
}
