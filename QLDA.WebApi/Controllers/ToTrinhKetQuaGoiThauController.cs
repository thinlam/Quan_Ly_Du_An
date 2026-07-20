using QLDA.Application.DuAns.Commands;
using BuildingBlocks.Domain.Entities;
using BuildingBlocks.Application.Attachments.Commands;
using QLDA.Application.TepDinhKems.DTOs;
using BuildingBlocks.Application.Attachments.Queries;
using BuildingBlocks.Application.Attachments.Common;
using QLDA.Application.ToTrinhKetQuaGoiThauMappings;
using QLDA.Application.ToTrinhKetQuaGoiThaus.Commands;
using QLDA.Application.ToTrinhKetQuaGoiThaus.DTOs;
using QLDA.Application.ToTrinhKetQuaGoiThaus.Queries;
using QLDA.WebApi.Models.ToTrinhKetQuaGoiThaus;
using System.Net.Mime;

namespace QLDA.WebApi.Controllers;

[Route("api/to-trinh-ket-qua-goi-thau")]
[Tags("Tờ trình phê duyệt kết quả gói thầu")]
public class ToTrinhKetQuaGoiThauController(IServiceProvider serviceProvider) : AggregateRootController(serviceProvider)
{
    [ProducesResponseType<ResultApi<ToTrinhKetQuaGoiThauDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ResultApi>(StatusCodes.Status400BadRequest)]
    [HttpGet("{id}/chi-tiet")]
    public async Task<ResultApi> Get(Guid id)
    {
        var entity = await Mediator.Send(new ToTrinhKetQuaGoiThauGetQuery()
        {
            Id = id,
            ThrowIfNull = true,
            IsNoTracking = true
        });

        var danhSachTepDinhKem = (await Mediator.Send(new GetAttachmentsQuery(
            GroupIds: [entity.Id.ToString()],
            IncludeSigned: false
        ))).ToAttachmentEntities();

        return ResultApi.Ok(entity.ToModel(danhSachTepDinhKem.ToList()));
    }

    [ProducesResponseType<ResultApi<IHasKey<Guid>>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ResultApi>(StatusCodes.Status400BadRequest)]
    [HttpDelete("{id}/xoa")]
    public async Task<ResultApi> Delete(Guid id)
    {
        var res = await Mediator.Send(new ToTrinhKetQuaGoiThauDeleteCommand(id));
        return ResultApi.Ok(res);
    }

    [ProducesResponseType<ResultApi<IHasKey<Guid>>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ResultApi>(StatusCodes.Status400BadRequest)]
    [HttpPost("them-moi")]
    [Consumes(MediaTypeNames.Application.Json)]
    public async Task<ResultApi> Create( [FromBody] ToTrinhKetQuaGoiThauInsertDto dto, [FromServices] IUnitOfWork unitOfWork,  CancellationToken cancellationToken = default)
    {
        var step = await Mediator.Send(new DuAnUpdateStepCommand(dto.DuAnId, dto.BuocId));
        await Mediator.Send(new DuAnUpdatePhaseCommand(dto.DuAnId, step));

        var entity = await Mediator.Send(new ToTrinhKetQuaGoiThauInsertCommand(dto), cancellationToken);
        List<Attachment> files = [.. dto.DanhSachTepDinhKem?.ToEntities(entity.Id, EGroupType.PheDuyetKetQuaGoiThauDuAn) ?? []];
        await Mediator.Send(new AttachmentBulkInsertOrUpdateCommand
        {
            GroupId = entity.Id.ToString(),
            GroupTypes = [nameof(EGroupType.PheDuyetKetQuaGoiThauDuAn)],
            Entities = files,
            AutoDeleteMissing = true
        }, cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return ResultApi.Ok(new { entity.Id });
    }

    [ProducesResponseType<ResultApi<ToTrinhKetQuaGoiThauDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ResultApi>(StatusCodes.Status400BadRequest)]
    [HttpPut("cap-nhat")]
    [Consumes(MediaTypeNames.Application.Json)]
    public async Task<ResultApi> Update([FromBody] ToTrinhKetQuaGoiThauInsertDto dto, [FromServices] IUnitOfWork unitOfWork, CancellationToken cancellationToken = default)
    {
        var entity = await Mediator.Send(new ToTrinhKetQuaGoiThauUpdateCommand(dto), cancellationToken);

        List<Attachment> files = [.. dto.DanhSachTepDinhKem?.ToEntities(entity.Id, EGroupType.PheDuyetKetQuaGoiThauDuAn) ?? []];
        await Mediator.Send(new AttachmentBulkInsertOrUpdateCommand
        {
            GroupId = entity.Id.ToString(),
            GroupTypes = [nameof(EGroupType.PheDuyetKetQuaGoiThauDuAn)],
            Entities = files,
            AutoDeleteMissing = true
        }, cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        var danhSachTepDinhKem = (await Mediator.Send(new GetAttachmentsQuery(
            GroupIds: [entity.Id.ToString()],
            IncludeSigned: false
        ), cancellationToken)).ToAttachmentEntities();

        return ResultApi.Ok(entity.ToDto(danhSachTepDinhKem.ToList()));
    }

    [ProducesResponseType<ResultApi<PaginatedList<ToTrinhKetQuaGoiThauDto>>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ResultApi>(StatusCodes.Status400BadRequest)]
    [HttpGet("danh-sach-tien-do")]
    public async Task<ResultApi> Get([FromQuery] ToTrinhKetQuaGoiThauSearchDto dto)
    {
        var res = await Mediator.Send(new ToTrinhKetQuaGoiThauDanhSachQuery()
        {
            IsNoTracking = true,
            DuAnId = dto.DuAnId,
            BuocId = dto.BuocId,
            PageSize = dto.PageSize??1,
            PageIndex = dto.PageIndex??10,
            GlobalFilter = dto.GlobalFilter,
            So = dto.So,
            TuNgay = dto.TuNgay,
            DenNgay = dto.DenNgay,
            LoaiDuAnTheoNamId = dto.LoaiDuAnTheoNamId,
        });
        return ResultApi.Ok(res);
    }
}
