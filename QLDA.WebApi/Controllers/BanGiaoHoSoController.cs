using System.Net.Mime;
using QLDA.Application.BanGiaoHoSos.Commands;
using QLDA.Application.BanGiaoHoSos.DTOs;
using QLDA.Application.BanGiaoHoSos.Queries;
using QLDA.Application.TepDinhKems.Commands;
using QLDA.Application.TepDinhKems.Queries;
using QLDA.WebApi.Models.BanGiaoHoSos;

namespace QLDA.WebApi.Controllers;

[ApiController]
[Route("api/ban-giao-ho-so")]
[Tags("Bàn giao hồ sơ")]
[Authorize]
public class BanGiaoHoSoController(IServiceProvider sp) : AggregateRootController(sp) {
    private readonly IMediator _mediator = sp.GetRequiredService<IMediator>();

    [HttpGet("{id}/chi-tiet")]
    [ProducesResponseType<ResultApi<BanGiaoHoSoModel>>(StatusCodes.Status200OK)]
    public async Task<ResultApi> Get(Guid id) {
        var entity = await _mediator.Send(new BanGiaoHoSoGetQuery(id));
        // Tải cả 2 loại tệp đính kèm
        var allFiles = await _mediator.Send(new GetDanhSachTepDinhKemQuery { 
            GroupId = [entity.Id.ToString()],
            EGroupTypes = [
                Domain.Enums.EGroupType.BanGiaoHoSo.ToString(),
                Domain.Enums.EGroupType.BienBanBanGiao.ToString()
            ]
        });
        var tepHS = allFiles.Where(f => f.GroupType == Domain.Enums.EGroupType.BanGiaoHoSo.ToString()).ToList();
        var bienBan = allFiles.Where(f => f.GroupType == Domain.Enums.EGroupType.BienBanBanGiao.ToString()).ToList();
        return ResultApi.Ok(entity.ToModel(tepHS, bienBan));
    }

    [HttpGet("danh-sach")]
    [ProducesResponseType<ResultApi<PaginatedList<BanGiaoHoSoDto>>>(StatusCodes.Status200OK)]
    public async Task<ResultApi> GetList([FromQuery] BanGiaoHoSoSearchDto searchDto, 
        [FromQuery] AggregateRootPagination pagination) {
        var res = await _mediator.Send(new BanGiaoHoSoGetDanhSachQuery {
            SearchDto = searchDto,
            PageIndex = pagination.PageIndex,
            PageSize = pagination.PageSize,
        });
        return ResultApi.Ok(res);
    }

    [HttpPost("them-moi")]
    [Consumes(MediaTypeNames.Application.Json)]
    [ProducesResponseType<ResultApi<Guid>>(StatusCodes.Status200OK)]
    public async Task<ResultApi> Insert([FromBody] BanGiaoHoSoInsertDto dto) {
        var entity = await _mediator.Send(new BanGiaoHoSoInsertCommand(dto));

        await _mediator.Send(new TepDinhKemBulkInsertOrUpdateCommand {
            GroupId = entity.Id.ToString(),
            Entities = dto.GetDanhSachTepHSBanGiao(entity.Id)
        });

        return ResultApi.Ok(entity.Id);
    }

    [HttpPut("cap-nhat")]
    [Consumes(MediaTypeNames.Application.Json)]
    [ProducesResponseType<ResultApi<Guid>>(StatusCodes.Status200OK)]
    public async Task<ResultApi> Update([FromBody] BanGiaoHoSoUpdateModel dto) {
        var entity = await _mediator.Send(new BanGiaoHoSoUpdateCommand(dto));

        await _mediator.Send(new TepDinhKemBulkInsertOrUpdateCommand {
            GroupId = entity.Id.ToString(),
            Entities = dto.GetDanhSachTepHSBanGiao(entity.Id)
        });

        return ResultApi.Ok(entity.Id);
    }

    [HttpPut("{id}/ban-giao")]
    [Consumes(MediaTypeNames.Application.Json)]
    [ProducesResponseType<ResultApi<int>>(StatusCodes.Status200OK)]
    public async Task<ResultApi> BanGiao(Guid id, [FromBody] BanGiaoHoSoBanGiaoModel model) {
        var bienBanEntities = model.GetDanhSachBienBanBanGiao(id);

        var entity = await _mediator.Send(new BanGiaoHoSoBanGiaoCommand(id, model.NgayBanGiao, model.PhongBanNhanId));

        // Lưu biên bản bàn giao (EGroupType.BienBanBanGiao)
        await _mediator.Send(new TepDinhKemBulkInsertOrUpdateCommand {
            GroupId = entity.Id.ToString(),
            Entities = bienBanEntities
        });

        return ResultApi.Ok(1);
    }

    [HttpDelete("{id}/xoa-tam")]
    [ProducesResponseType<ResultApi<int>>(StatusCodes.Status200OK)]
    public async Task<ResultApi> SoftDelete(Guid id) {
        await _mediator.Send(new BanGiaoHoSoDeleteCommand(id));
        return ResultApi.Ok(1);
    }
}
