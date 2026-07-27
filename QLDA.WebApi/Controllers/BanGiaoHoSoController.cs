using System.Net.Mime;
using QLDA.Application.BanGiaoHoSos.Commands;
using QLDA.Application.BanGiaoHoSos.DTOs;
using QLDA.Application.BanGiaoHoSos.Queries;
using QLDA.Application.TepDinhKems.DTOs;
using BuildingBlocks.Application.Attachments.Commands;
using BuildingBlocks.Application.Attachments.Queries;
using BuildingBlocks.Application.Attachments.Common;
using QLDA.WebApi.Models.BanGiaoHoSos;
using QLDA.WebApi.Models.TepDinhKems;

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
        // Mặc định IncludeSigned=true → gốc + KySo_*; split bằng ToBaseGroupType
        var allFiles = (await _mediator.Send(new GetAttachmentsQuery(
            GroupIds: [entity.Id.ToString()],
            BaseGroupTypes: [
                nameof(EGroupType.BanGiaoHoSo),
                nameof(EGroupType.BienBanBanGiao)
            ]
            //IncludeSigned = false < nếu ko muốn load file kí sổ>
        ))).ToAttachmentEntities();
        var tepHS = allFiles
            .Where(f => f.GroupType.ToBaseGroupType() == nameof(EGroupType.BanGiaoHoSo))
            .ToList();
        var bienBan = allFiles
            .Where(f => f.GroupType.ToBaseGroupType() == nameof(EGroupType.BienBanBanGiao))
            .ToList();
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

        await _mediator.Send(new AttachmentBulkInsertOrUpdateCommand {
            GroupId = entity.Id.ToString(),
            GroupTypes = [nameof(EGroupType.BanGiaoHoSo)],
            Entities = dto.DanhSachTepDinhKem?.ToEntities(entity.Id, EGroupType.BanGiaoHoSo).ToList() ?? [],
            AutoDeleteMissing = true
        });

        return ResultApi.Ok(entity.Id);
    }

    [HttpPut("cap-nhat")]
    [Consumes(MediaTypeNames.Application.Json)]
    [ProducesResponseType<ResultApi<Guid>>(StatusCodes.Status200OK)]
    public async Task<ResultApi> Update([FromBody] BanGiaoHoSoUpdateModel dto) {
        var entity = await _mediator.Send(new BanGiaoHoSoUpdateCommand(dto));

        await _mediator.Send(new AttachmentBulkInsertOrUpdateCommand {
            GroupId = entity.Id.ToString(),
            GroupTypes = [nameof(EGroupType.BanGiaoHoSo)],
            Entities = dto.DanhSachTepDinhKem?.ToEntities(entity.Id, EGroupType.BanGiaoHoSo).ToList() ?? [],
            AutoDeleteMissing = true
        });

        return ResultApi.Ok(entity.Id);
    }

    [HttpPut("{id}/ban-giao")]
    [Consumes(MediaTypeNames.Application.Json)]
    [ProducesResponseType<ResultApi<int>>(StatusCodes.Status200OK)]
    public async Task<ResultApi> BanGiao(Guid id, [FromBody] BanGiaoHoSoBanGiaoModel model) {
        var bienBanEntities = model.DanhSachBienBan?.ToEntities(id, EGroupType.BienBanBanGiao).ToList() ?? [];

        var entity = await _mediator.Send(new BanGiaoHoSoBanGiaoCommand(id, model.NgayBanGiao, model.PhongBanNhanId));

        await _mediator.Send(new AttachmentBulkInsertOrUpdateCommand {
            GroupId = entity.Id.ToString(),
            GroupTypes = [nameof(EGroupType.BienBanBanGiao)],
            Entities = bienBanEntities,
            AutoDeleteMissing = true
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
